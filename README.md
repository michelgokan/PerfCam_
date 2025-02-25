# PerfCam: Digital Twinning for Production Lines using 3D Gaussian Splatting and Vision Models

**[KTH Royal Institute of Technology, SCI](https://www.kth.se/en/sci/skolan-for-teknikvetenskap-1.795005)**; *
*[AstraZeneca, Sweden Operations](https://www.astrazeneca.com/)**

[Michel Gokan Khan](https://michelgokan.github.io/), [Renan Guarese](https://renghp.github.io/), [Fabian Johonsson](https://se.linkedin.com/in/fabianmartinjohnson), [Xi Vincent Wang](https://www.kth.se/profile/wangxi), [Anders Bergman](https://se.linkedin.com/in/anders-bergman-186203), [Benjamin Edvinsson](https://se.linkedin.com/in/benjamin-edvinsson-860ba968), [Mario Romero Vega](https://www.kth.se/profile/marior), [Jérémy Vachier](https://github.com/jvachier), [Jan Kronqvist](https://www.kth.se/profile/jankr)

[[`Paper`](#)] [[`Dataset`](https://github.com/AstraZeneca/PerfCam-Dataset)] [[
`Project`](https://www.digitalfutures.kth.se/project/smart-smart-predictive-maintenance-for-the-pharmaceutical-industry/)] [[
`BibTeX`](#citing-smart)]

Welcome to **PerfCam** Proof-of-Concept (PoC) repository, the official codebase accompanying the paper:

This repository provides:

1. **3D printable files** (STLs) and references for assembling the PerfCam hardware (camera + servo system).
2. **Vision module** code and instructions for training YOLOv11 models and generating annotations (including instructons
   on preparing COLMAP point clouds).
3. **Unity project** for the digital twin demonstration, showcasing real-time integration of annotated camera feeds and
   production line analytics.

---

## Table of Contents

1. [Overview](#overview)
2. [Repository Structure](#repository-structure)
3. [Hardware Assembly Instructions](#hardware-assembly-instructions)
4. [Vision & Training Instructions](#vision--training-instructions)
5. [Unity Digital Twin](#unity-digital-twin)
6. [Dataset](#dataset)
7. [License](#license)
8. [Contributors](#contributors)
9. [Citation](#citation)

---

## Overview

**PerfCam** leverages:

- A **camera module** with an Intel RealSense D435i for capturing product images from various angles.
- **3D Gaussian Splatting** techniques for generating dense point clouds and digital twins.
- **YOLOv11** for object detection and product annotation.
- A **Unity**-based digital twin interface that visualizes production line efficiency and real-time data (e.g., OEE,
  throughput).

**Goal**: Provide a scalable and flexible system for digital twinning in industrial environments. Please see the paper
for more info.

---

## Repository Structure

    PerfCam/
    ├── camera_module/
    │   ├── stls/
    │   │   └── (Contains STL files for the rear closure of the RealSense case)
    │   └── rpi5/
    │       └── (Python scripts for Raspberry Pi 5 to control servo motors)
    ├── vision/
    │   ├── 3d_reconstruction/
    │   │   ├── SuGar.def (Aptainer/Singularity definition file)
    │   │   ├── colmap/
    │   │   │   └── (Contains setup file and instruction on how to run the GUI)  
    │   │   └── SuGaR/
    │   │       └── (Contains setup file and instruction on how to run the GUI)
    │   └── yolo/
    │       └── (YOLOv11 scripts and Roboflow workflow definitions)
    └── unity/
        └── (Unity project for the digital twin demonstration)

- **camera_module**
    - **stls**: Contains the custom STL for the rear closure of the Intel RealSense D435 case.
    - **rpi5**: Python code to control the servo motors (pan and tilt) on a Raspberry Pi 5.

- **vision**
    - **colmap**: Files (especially the Aptainer definition) to run Colmap on HPC environments (e.g., NAISS / ALVIS).
    - **yolo**: YOLOv11 training scripts, model configuration, and a Roboflow workflow file.

- **unity_project**
    - Contains the Unity-based digital twin that displays real-time production line metrics and annotated camera feeds.

---

## Hardware Assembly Instructions

1. **Base Mechanism**
    - For the base, we used an existing gimbal design
      from [Thangs.com](https://thangs.com/designer/HowToMechatronics/3d-model/Self%20balancing%20platform-43942),
      designed by [HowToMechatronics](https://thangs.com/designer/HowToMechatronics).
    - Follow
      its [assembly instructions](https://howtomechatronics.com/projects/diy-arduino-gimbal-self-stabilizing-platform/#diy-arduino-gimbal-self-stabilizing-platform-stl-files)
      in that project. You can find the link to STL files in the instructions. **Do NOT print** the final platform ("`Top Platform.STL`") part from that design.

2. **Intel RealSense D435 Case**
    - Download the case from [GrabCAD.com](https://grabcad.com/library/intel-realsense-d435-case-1), (designed
      by [Elena Bertucci](https://grabcad.com/elena.bertucci-1)). Only print the *front part* (i.e., the main housing "
      `Case D435i.stl`").
    - Skip printing the *rear closure* from that GrabCAD design.

3. **Rear Closure STL**
    - Use the file located in `camera_module/stls/`. This custom-designed rear closure is specifically for the PerfCam
      setup.
    - Print this and attach it to the D435i main case.

4. **Final Assembly**
    - Attach the D435i camera (with the newly printed rear closure) onto the servo platform following the Thangs
      mechanism (instead of the platform).
    - Wire the servo motors to your Raspberry Pi 5 using a breadboard according to
      the [assembly instructions](https://howtomechatronics.com/projects/diy-arduino-gimbal-self-stabilizing-platform/#diy-arduino-gimbal-self-stabilizing-platform-stl-files).
    - Instead of using Arduino used in the instructions, we used Rasperry Pi 5, the instructions is similar and make
      sure you connect the ground pins correctly. [This video](https://www.youtube.com/watch?v=C07i3LBRzOs) provides a
      nice overview on how to setup the wiring.
    - The GPIO PIN mapping we've used are as follows:
        - The base servo motor (yaw axis): GPIO 22 (PIN 15)
        - The second servo (roll axis): GPIO 17 (PIN 11)
        - The third servo (pitch axis): GPIO 27 (13)
    - We've also used a single 2.6V Red LED 5mm Through Hole (optional) that may blink during running Python script. For
      that, we've used GPIO 14 (PIN 8).

---

## Setting up Raspberry Pi 5

We've used and test the code under a Ubuntu 24.04.1 LTS on top of a Raspberry Pi 5.

You can find a setup script under `camera_module/rpi5/setup-rpi.sh`. The setup script will help you install/build
several packages needed for running the main script, including Open3D, librealsense, and pigpio. However, for
development and test purposes, it additionally installs/builds useful packages required for additional processing and
testing the dataset, including octomap, and ROS 2 (Jazzy) as well as a related workspace, but you can comment out those
sections in the setup script if you do not need them in your workspace.

The python code, `camera_module/rpi5/run-dataset-creation.py` has only been used to generate the dataset reported in the
paper. It rotates servo motors, takes snapshots, and stores the frames, IMU, and motors angle information under a
dataset folder.

To run the script, navigate to `camera_module/rpi5` and then run following:

```bash
LD_PRELOAD=/usr/local/lib/libOpen3D.so python run-dataset-creation.py
```

---

## Vision & Training Instructions

1. **Build The Image**:
    - Navigate to `vision/`.
    - Use the provided Aptainer definition (`.def` file) to build and run a container that
      includes [COLMAP](https://colmap.github.io/) and [SuGaR](https://github.com/Anttwo/SuGaR).
   ```bash
   apptainer build --fakeroot --nv SuGaR.sif SuGaR.def
   ```
2. **COLMAP Point Cloud Extraction**
    - Run your reconstruction/point cloud extraction following the [following instructions](https://colmap.github.io/tutorial.html#quickstart).
    - This approach was tested on the Swedish NAISS (ALVIS) HPC environment.
    - You can run the GUI using the following command (follow this to create
      a [persistent overlay](https://apptainer.org/docs/user/main/cli/apptainer_overlay_create.html), if needed):
   ```bash
   apptainer exec --nv --fakeroot --overlay overlay.img SuGaR.sif colmap gui
   ```

3. **YOLOv11 Model Training**
    - Navigate to `vision/yolo/`.
    - Create a virtual environment, following [this instruction](https://docs.python.org/3/library/venv.html).
    - Inside, you will find:
        - **Roboflow workflows** configuration file that automates dataset versioning and annotation steps under `vision/yolo/roboflow/`.
        - **Training scripts** for YOLOv11 (with hyperparameters and dataset splitting), there is a Google Colab notebook as well as the python code under `vision/yolo/train/` folder.
    - Update paths and environment variables as needed, then train your model

4. **Roboflow Workflows**
    - Navigate to `vision/yolo/roboflow`.
    - Inside, you will find 4 workflow files, each for one camera.
    - Instructions on how to import and run the workflows in Roboflow can be
      found [here]([https://roboflow.com/](https://docs.roboflow.com/workflows/create-a-workflow)) (you need to first
      create 4 workflows and replace the json files provided).
    - To run the workflow, simply adjust the `run-workflow.py` to map the correct the workflow ID, project, and use right API KEY:
    ```python
   ...
    # initialize a pipeline object
    pipeline = InferencePipeline.init_with_workflow(
        api_key="<YOUR_APU_KEY>",
        workspace_name="<YOUR_WORKSPACE_NAME>",
        workflow_id=workflow_id,
        video_reference=video_path,
        max_fps=30,
        on_prediction=video_saver.sink,
    )
    ...
    ```
    ...and then run following:
    ```bash
   cd vision/yolo/
   python run-workflow.py
   ```
    
5. **Train 3D Gaussian Splatting Model From Scratch**
   - We've used ["Surface-Aligned Gaussian Splatting for Efficient 3D Mesh Reconstruction and High-Quality Mesh Rendering"](https://github.com/Anttwo/SuGaR) method for extracting 3D Gaussians. Follow their instructions on how to train your model based on COLMAP output.
   - If you wish to run it using a container, there is a `vision/3d_reconstruction/3dgs/setup.sh` that would help you run the 3DGS training in a container based on the extracted dataset. For more info, follow [this instruction](https://github.com/Anttwo/SuGaR?tab=readme-ov-file#quick-start).
---

## Unity Digital Twin

- The `unity/` folder contains the Unity scene and scripts used for:
    - Visualizing production line metrics (e.g., OEE) and throughput based on the CSVs.
    - Displaying annotated camera feeds from the YOLOv11 model outputs.
    - Mapping recognized products (from the camera) to digital twins in the scene.

> **Note**: This is a proof of concept; not every single parameter from the dataset is connected. Key metrics are
> integrated to demonstrate how the system functions.

We've used [UnityGaussianSplatting](https://github.com/aras-p/UnityGaussianSplatting) package to load the SuGaR models (will be cloned as a submodule of this repo).

Please make sure you initialize Git submodules before anything else:

```
git submodule update --init --recursive --force
```

Also, please make sure you download the dataset using the `install.sh` script in the root directory in order to load the statistics and products in the digital twin.

---

## Dataset

We have released a separate **PerfCam dataset** containing:

- Raw images, annotated views, and additional metadata used in training and evaluation.
- 3D scans, point clouds, and sample reconstructions.

You can find the link in the paper's main webpage.

---

## LFS Considerations
To optimize this repository's performance and ensure efficient handling of large files, make sure you track the following file types with Git LFS:

```
git lfs track **/*.bytes
```

---

## License

This project is licensed under the [Apache License 2.0](LICENSE).  
See the [LICENSE](LICENSE) file for details.

---

## Code Contributors

- [Michel Gokon Khan](https://michelgokan.github.io/) (Main Contributor)
- [Renan Guarese](https://renghp.github.io/) (Contributed on Unity Visualizations)

Contributions, bug reports, and feature requests are welcome. Please open a issue in case of problems.

---

## Citation

Please cite this work if you use any part of this repository in your research:

```bibtex
@article{PerfCam2025,
  title     = {PerfCam: Digital Twinning for Production Lines using 3D Gaussian Splatting and Vision Models},
  author    = {Michel Gokon Khan and et. al.},
  booktitle = {To Be Added},
  year      = {2025},
  pages     = {--}
}
```

---

## Questions

Thank you for your interest in PerfCam. For any questions or issues, please open an issue or contact the contributors.
