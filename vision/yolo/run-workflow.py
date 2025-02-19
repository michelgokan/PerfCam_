import csv
import os
from datetime import timedelta

import cv2
from inference import InferencePipeline  # Import the InferencePipeline object


class VideoSaver:
    def __init__(self, output_path, fps, frame_size, csv_folder_path):
        self.output_path = output_path
        self.csv_folder_path = csv_folder_path
        self.fps = fps
        self.frame_size = frame_size
        fourcc = cv2.VideoWriter_fourcc(*'mp4v')
        self.out = cv2.VideoWriter(self.output_path, fourcc, self.fps, self.frame_size)

        if not self.out.isOpened():
            raise ValueError(f"Failed to open VideoWriter with path: {self.output_path}")
        print(f"VideoWriter initialized with FPS={self.fps}, Size={self.frame_size}")

        # Create CSV folder if it doesn't exist
        os.makedirs(self.csv_folder_path, exist_ok=True)

        # Initialize CSV file
        self.init_csv('bounding_boxes.csv', ['Timestamp', 'Bounding Box', 'Confidence', 'Class Name'])
        self.init_csv('edge_counters.csv', ['Timestamp', 'Counter Name', 'Count In', 'Count Out'])

    def init_csv(self, filename, headers):
        with open(os.path.join(self.csv_folder_path, filename), mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(headers)

    def sink(self, result, video_frame):
        if result.get("output_image"):
            output_image = result["output_image"].numpy_image

            # Ensure the frame size matches
            if (output_image.shape[1], output_image.shape[0]) != self.frame_size:
                output_image = cv2.resize(output_image, self.frame_size)

            self.out.write(output_image)  # Write the frame to the video file

        # Calculate timestamp
        frame_number = video_frame.frame_id if hasattr(video_frame, 'frame_id') else 0
        total_seconds = (frame_number - 1) / self.fps
        td = timedelta(seconds=total_seconds)
        timestamp = f"{td.seconds // 3600:02}:{(td.seconds // 60) % 60:02}:{td.seconds % 60:02}.{td.microseconds // 1000:03}"
        # timestamp = str(timedelta(seconds=total_seconds))[:-3]  # Format in HH:MM:SS.SSS

        # Write bounding boxes to CSV
        detections = result.get('predictions', None)
        if detections:
            bounding_boxes = detections.xyxy.tolist()
            confidences = detections.confidence.tolist()
            class_names = detections.data.get('class_name').tolist()

            with open(os.path.join(self.csv_folder_path, 'bounding_boxes.csv'), mode='a', newline='') as file:
                writer = csv.writer(file)
                for box, confidence, class_name in zip(bounding_boxes, confidences, class_names):
                    writer.writerow([timestamp, box, confidence, class_name])

        # Write edge counters to CSV
        for key, value in result.items():
            if 'counter' in key:
                try:
                    count_in = value["count_in"]
                    count_out = value["count_out"]
                except TypeError:
                    print(f"Key {key} does not have count_in or count_out")
                    continue

                with open(os.path.join(self.csv_folder_path, 'edge_counters.csv'), mode='a', newline='') as file:
                    writer = csv.writer(file)
                    writer.writerow([timestamp, key, count_in, count_out])
        print(result)

    def release(self):
        if self.out:
            self.out.release()
            print("VideoWriter released and video saved.")


def get_video_fps(video_path):
    cap = cv2.VideoCapture(video_path)
    if not cap.isOpened():
        raise ValueError(f"Error opening video file: {video_path}")
    fps = cap.get(cv2.CAP_PROP_FPS)
    frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    cap.release()
    return fps, (frame_width, frame_height)


def main():
    camera_id = "3"  # The Camera ID (we used numberic values, see dataset/paper for more info)
    video_path = f"{camera_id}.mp4"
    output_video_path = f"{camera_id}-annotated.mp4"
    csv_folder_path = f"camera-{camera_id}"
    workflow_id = f"camera-{camera_id}"

    original_fps, frame_size = get_video_fps(video_path)
    print(f"Original Video FPS: {original_fps}, Frame Size: {frame_size}")

    video_saver = VideoSaver(output_path=output_video_path, fps=original_fps, frame_size=frame_size,
                             csv_folder_path=csv_folder_path)

    # initialize a pipeline object
    pipeline = InferencePipeline.init_with_workflow(
        api_key="<YOUR_APU_KEY>",
        workspace_name="<YOUR_WORKSPACE_NAME>",
        workflow_id=workflow_id,
        video_reference=video_path,
        max_fps=30,
        on_prediction=video_saver.sink,
    )

    try:
        pipeline.start()  # start the pipeline
        pipeline.join()  # wait for the pipeline thread to finish
    finally:
        video_saver.release()


if __name__ == "__main__":
    main()
