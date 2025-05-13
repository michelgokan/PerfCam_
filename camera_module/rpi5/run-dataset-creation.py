import copy
import json
import math
import os
import pickle
import sys
from time import sleep

import cv2
import numpy as np
import open3d as o3d
import pyrealsense2 as rs
from gpiozero import AngularServo, LED
from matplotlib import pyplot as plt
from realsense_depth import DepthCamera

sys.path.append('..')

# Initialize LED and Servos
led = LED(14)
resolution_width, resolution_height = (1280, 720)
clip_distance_max = 2  # Maximum clipping distance in meters

# Define voxel size and radius for normal estimation
voxel_size = 0.01  # Adjust based on your data
radius_normal = voxel_size * 2


def convert_to_rgbd_image(color_raw_frame, depth_raw_frame):
    color_image = np.asanyarray(color_raw_frame.get_data())
    depth_image = np.asanyarray(depth_raw_frame.get_data())

    # Convert frames to Open3D Images
    color_o3d = o3d.geometry.Image(color_image)
    depth_o3d = o3d.geometry.Image(depth_image)

    rgbd_image = o3d.geometry.RGBDImage.create_from_color_and_depth(
        color_o3d, depth_o3d,
        depth_scale=1.0 / depth_scale,
        depth_trunc=clip_distance_max,
        convert_rgb_to_intensity=False
    )
    return rgbd_image


# Servo initialization (unchanged from your code)
servos = [
    AngularServo(22, min_angle=0, max_angle=180, min_pulse_width=0.5 / 1000, max_pulse_width=2.5 / 1000),
    AngularServo(17, min_angle=0, max_angle=180, min_pulse_width=0.5 / 1000, max_pulse_width=2.5 / 1000,
                 initial_angle=0),
    AngularServo(27, min_angle=0, max_angle=180, min_pulse_width=0.5 / 1000, max_pulse_width=2.5 / 1000,
                 initial_angle=90)
]

sleep(1)

for servo in servos:
    servo.detach()

led.blink()


def set_angle(servo, angle, angle_range, servo_id):
    clamped_angle = max(min(angle, angle_range[1]), angle_range[0])
    servo.angle = clamped_angle
    sleep(0.5)
    servo.detach()
    sleep(0.5)
    print("set servo " + str(servo_id) + " to " + str(angle) + " degree (clamped = " + str(clamped_angle) + ")")


# Remove contents of data folder if needed
delete_data = input("Delete contents of data folder? (y, n): ")
if delete_data.lower() == 'y':
    os.system("rm -rf data/*")
    os.system("mkdir -p data")
    os.system("touch data/.gitkeep")
    print("The data folder has been cleaned.")
    
# Phase 1: Decide whether to capture new frames or load previous shots

# Phase 1: Capturing Frames
print("Capturing frames...")
all_rgbd_images = []
all_results = []
all_angles = []

all_rgbd_images, all_results, all_angles = [], [], []  # Initialize as empty for new captures

print("Starting...")
Realsensed435Cam = DepthCamera(resolution_width, resolution_height)
depth_scale = Realsensed435Cam.get_depth_scale()
image_number = 1

# Get camera intrinsics from RealSense
profile = Realsensed435Cam.pipeline.get_active_profile()
intrinsics = profile.get_stream(rs.stream.color).as_video_stream_profile().get_intrinsics()
intrinsic = o3d.camera.PinholeCameraIntrinsic(
    intrinsics.width, intrinsics.height,
    intrinsics.fx, intrinsics.fy,
    intrinsics.ppx, intrinsics.ppy
)


def save_frame(ret, depth_raw_frame, color_raw_frame, image_number, camera, angle0, angle1, angle2):
    if not ret:
        print("Error: Failed to retrieve frame.")
        return None, None

    # Retrieve IMU data
    imu_data_file = f"data/frame_imu_{image_number}.json"
    imu_success = camera.store_imu_data_as_json(imu_data_file)
    if imu_success:
        print(f"IMU data saved to {imu_data_file}.")
    else:
        print("Failed to save IMU data.")

    # Save robot arm angles
    angles_data_file = f"data/motor_angles_{image_number}.json"
    angles_data = {
        "angle0": angle0,
        "angle1": angle1,
        "angle2": angle2
    }
    try:
        with open(angles_data_file, 'w') as json_file:
            json.dump(angles_data, json_file, indent=4)
        print(f"Robot arm angles saved to {angles_data_file}.")
    except Exception as e:
        print(f"Failed to save robot arm angles: {e}")

    color_frame = np.asanyarray(color_raw_frame.get_data())
    depth_frame = np.asanyarray(depth_raw_frame.get_data())

    print("Frame captured successfully. Color frame shape:", color_frame.shape)

    # Create RGBD image and point cloud
    rgbd_image = convert_to_rgbd_image(color_raw_frame, depth_raw_frame)

    # Save point cloud and images
    # o3d.io.write_point_cloud(f"data/capture_{image_number}.ply", pcd_down)
    cv2.imwrite(f"data/frame_color_{image_number}.png", color_frame)
    plt.imsave(f"data/frame_depth_{image_number}.png", depth_frame)

    image_number += 1
    # return pcd_down, rgbd_image
    return None, rgbd_image


start_reverse_input = input("Start in reverse? (y, N): ")
start_reverse = False
if start_reverse_input.lower() == 'y':
    start_reverse = True
    print("Okay, let's adjust camera in reverse mode...")

servos_range = [
    (0, 180),
    (0, 0),
    (63, 180)
]

first_servo_initial_angle = servos_range[0][1] if start_reverse else servos_range[0][0]
angle = first_servo_initial_angle
max_rotation_count = 200000  # Increased number of frames
angle_step = 18  # Reduced angle step for more overlap

direction = 1 if start_reverse else -1
direction2 = -1 if start_reverse else 1
third_servo_initial_angle = servos_range[2][1] if start_reverse else servos_range[2][0]
third_servo_angle = third_servo_initial_angle


try:
    second_servo_angle = servos_range[1][1] if start_reverse else servos_range[1][0]

    while angle <= servos_range[0][1] and angle >= servos_range[0][0]:
        if second_servo_angle > servos_range[1][1]:
            second_servo_angle = servos_range[1][0]

        set_angle(servos[1], second_servo_angle, servos_range[1], 1)

        servo = 0
        set_angle(servos[0], angle, servos_range[0], 0)
        all_angles.append(angle)

        direction = direction * -1

        while third_servo_angle >= servos_range[2][0] and third_servo_angle <= servos_range[2][1]:
            set_angle(servos[2], third_servo_angle, servos_range[2], 2)
            while True:
                print("Capturing new frame...wait half a sec to supress vibration")
                sleep(0.5)
                ret, depth_raw_frame, color_raw_frame = Realsensed435Cam.get_raw_frame()
                sleep(0.1)
                if ret:
                    break
                else:
                    print("Unable to get a frame")

            print("Saving frame...")
            new_pcd, rgbd_image = save_frame(ret, depth_raw_frame, color_raw_frame, image_number, Realsensed435Cam,
                                             angle, second_servo_angle, third_servo_angle)
            image_number += 1

            third_servo_angle += 18 * direction
            if third_servo_angle > servos_range[2][1]:
                third_servo_angle = servos_range[2][1]
                break
            if third_servo_angle < servos_range[2][0]:
                third_servo_angle = servos_range[2][0]
                break

        print("Current first servo angle = " + str(angle))
        angle += angle_step * direction2
        print("Now changed to " + str(angle))


except KeyboardInterrupt:
    print("Program stopped by user")
    Realsensed435Cam.release()
