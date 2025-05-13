#!/bin/bash

sudo swapoff -a
sudo fallocate -l 16G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile

sudo apt-get update
sudo apt-get upgrade
sudo apt-get install wget cmake build-essential vim geany git libxrandr2 libxrandr-dev libxinerama-dev libxcursor-dev libglu1-mesa-dev libx11-dev libssl-dev libxi-dev  libglfw3-dev  libgl1-mesa-dev at libudev-dev v4l-utils python3-pigpio python3.12 python3.12-dev python3-pip pipx python3-virtualenv python-is-python3 pigpio-tools python3-setuptools python3-gpiozero f3d nfs-kernel-server gh qtbase5-dev libqt5opengl5-dev libqglviewer-dev-qt5 curl software-properties-common htop gcc-14 libc++-18-dev clang-18 gfortran libstdc++-14-dev libxxf86vm-dev


# for pycharm
sudo apt-get install nodejs default-jre

pip install gpiozero pigpio

mkdir ~/PerfCam
cd ~/PerfCam
git clone https://github.com/IntelRealSense/librealsense
cd librealsense
git checkout v2.56.1
./scripts/setup_udev_rules.sh
 ./scripts/patch-realsense-ubuntu-lts-hwe.sh
mkdir build
cd build
cmake .. -DBUILD_EXAMPLES=true -DBUILD_WITH_OPENMP=false -DHWM_OVER_XU=false -DBUILD_PYTHON_BINDINGS=ON -DPYTHON_EXECUTABLES=$(which python3)

make

sudo cp ./Release/*.so /usr/local/lib/
export PYTHONPATH=$PYTHONPATH:/usr/local/lib

sudo make install

echo "Add the following to your .bashrc"
echo "export PYTHONPATH=\$PYTHONPATH:/usr/local/lib"

cd ../..

sudo rm /usr/lib/python3.12/EXTERNALLY-MANAGED 

git clone https://github.com/isl-org/Open3D
cd Open3D
mkdir build
cd build
echo "In case of error, run the following command..."
echo "sed -i 's/M_PIf/M_PI_f/g' ./filament/src/ext_filament/libs/image/src/ImageSampler.cpp"
sed -i 's/M_PIf/M_PI_f/g' ./filament/src/ext_filament/libs/image/src/ImageSampler.cpp
 cmake -DCMAKE_CXX_FLAGS="-Wno-error=free-nonheap-object" -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=ON -DBUILD_FILAMENT_FROM_SOURCE=ON -DPYTHON_EXECUTABLE=/home/m/PerfCam/.venv/bin/python3 ..
echo "Maybe just make without -j..."
make -j$(nproc)
sudo make install
sudo make install-pip-package
sudo make python-package
sudo make pip-package

cd ../..
wget https://github.com/joan2937/pigpio/archive/master.zip
unzip master.zip
rm -Rf master.zip
cd pigpio-master
make
sudo make install

cd ..
git clone https://github.com/WiringPi/WiringPi
cd WiringPi
./build debian
mv debian-template/wiringpi-3.0-1.deb .
sudo apt install ./wiringpi-3.0-1.deb


pip install matplotlib opencv-python numpy


# Install OctoMap
cd ..
git clone https://github.com/OctoMap/octomap
cd octomap
mkdir build
cd build
cmake ..
make -j$(nproc)
sudo make install
cd ../octovis
mkdir build
cd build
cmake ..
make
sudo make install

# Install ROS Jazzy
locale  # check for UTF-8
sudo apt update && sudo apt install locales
sudo locale-gen en_US en_US.UTF-8
sudo update-locale LC_ALL=en_US.UTF-8 LANG=en_US.UTF-8
export LANG=en_US.UTF-8
locale  # verify settings
sudo add-apt-repository universe
sudo apt update
sudo curl -sSL https://raw.githubusercontent.com/ros/rosdistro/master/ros.key -o /usr/share/keyrings/ros-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/ros-archive-keyring.gpg] http://packages.ros.org/ros2/ubuntu $(. /etc/os-release && echo $UBUNTU_CODENAME) main" | sudo tee /etc/apt/sources.list.d/ros2.list > /dev/null
sudo apt update && sudo apt install -y ros-dev-tools
sudo apt upgrade
sudo apt-get install -y ros-jazzy-desktop ros-jazzy-grid-map-* liboctomap-dev ros-jazzy-ament-* ros-jazzy-tf2* ros-jazzy-nav* ros-jazzy-rtabmap* ros-jazzy-geometr* ros-jazzy-imu-tools ros-jazzy-realsense2* ros-jazzy-image-proc* ros-jazzy-rplidar* ros-jazzy-rosbridge-suite ros-jazzy-rosbridge-server

# Install RealSense ROS
source /opt/ros/jazzy/setup.bash
mkdir -p ~/ros2_ws/src
cd ~/ros2_ws/src/
git clone https://github.com/IntelRealSense/realsense-ros.git -b ros2-master
cd ~/ros2_ws
sudo rosdep init
rosdep update
sudo apt-get update
rosdep install -i --from-path src --rosdistro $ROS_DISTRO --skip-keys=librealsense2 -y
colcon build
. install/local_setup.bash

# To run the camera node
ros2 launch realsense2_camera rs_launch.py unite_imu_method:=linear_interpolation

# Kimera stuff
sudo apt-get install -y --no-install-recommends apt-utils
sudo apt-get install -y \
      cmake build-essential unzip pkg-config autoconf \
      libboost-all-dev \
      libjpeg-dev libpng-dev libtiff-dev \
     m libgtk-3-dev \
      libatlas-base-dev gfortran \
      libparmetis-dev libvtk9-dev catkin-tools
##### Kimera incomplete :(

cd ~/ros2_ws
git clone https://github.com/introlab/rtabmap.git src/rtabmap
git clone --branch ros2 https://github.com/introlab/rtabmap_ros.git src/rtabmap_ros
rosdep update && rosdep install --from-paths src --ignore-src -r -y
export MAKEFLAGS="-j6" # Can be ignored if you have a lot of RAM (>16GB)
source ~/ros2_ws/install/setup.bash
source /opt/ros/jazzy/setup.bash
sudo ln -s /usr/local/share/octomap/octomap-config.cmake /usr/local/share/octomap/octomapConfig.cmake
colcon build --symlink-install --cmake-args -DRTABMAP_SYNC_MULTI_RGBD=ON -DRTABMAP_SYNC_USER_DATA=ON -DCMAKE_BUILD_TYPE=Release

# LD_PRELOAD=/usr/local/lib/libOpen3D.so setup-rpi.sh <---- run this file like this
