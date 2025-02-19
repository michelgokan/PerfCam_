#!/bin/bash
# These modules used in the supercomputing facility we've used, feel free to comment them out if you already
# have them enabled in your environment.
module load PyTorch-bundle/2.1.2-foss-2023a-CUDA-12.1.1
module load Miniforge3/24.1.2-0
module load CUDA/12.1.1

# To build SuGaR container that also support COLMAP
apptainer build --fakeroot --nv SuGaR.sif SuGaR.def

####
# I manually changed SuGaR/gaussian_splatting/scene/__init__.py and added args.resolution = 1 to prevent
# downscaling the resolution
####

# To Run colmap GUI
apptainer exec --nv --fakeroot --overlay overlay.img SuGaR.sif colmap gui

# To shell into the SuGaR container
apptainer shell --nv --overlay overlay.img --fakeroot SuGaR.sif
