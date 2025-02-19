#!/bin/bash

# Script Name: install.sh
# Description: Installs the PerfCam dataset by downloading and extracting it.
# Usage: ./install.sh [--help]

# Dataset URL
DATASET_URL="https://github.com/AstraZeneca/PerfCam-Dataset/archive/refs/tags/v0.1.1.tar.gz"
DATASET_ARCHIVE="v0.1.1.tar.gz"
EXTRACT_DIR="perfcam-dataset"

# Function to display help message
function show_help() {
    echo "Usage: ./install.sh [--help]"
    echo
    echo "This script downloads and extracts the PerfCam dataset."
    echo "Options:"
    echo "  --help    Show this help message and exit."
}

# Function to log messages
function log_message() {
    local message=$1
    echo "[INFO] $message"
}

# Function to log errors
function log_error() {
    local message=$1
    echo "[ERROR] $message" >&2
}

# Check if the user requested help
if [[ "$1" == "--help" ]]; then
    show_help
    exit 0
fi

# Step 1: Download the dataset
# Make sure personal token is authorized to use with SAML:
# https://docs.github.com/en/enterprise-cloud@latest/authentication/authenticating-with-saml-single-sign-on/authorizing-a-personal-access-token-for-use-with-saml-single-sign-on
log_message "Starting download of the dataset from $DATASET_URL..."
if curl -u $GITHUB_USERNAME:$GITHUB_TOKEN -L -O -o "$DATASET_ARCHIVE" "$DATASET_URL"; then
    log_message "Dataset downloaded successfully."
else
    log_error "Failed to download the dataset. Please check your internet connection or the URL."
    exit 1
fi

# Step 2: Extract the dataset
log_message "Extracting the dataset..."
if tar -xzf "$DATASET_ARCHIVE"; then
    log_message "Dataset extracted successfully."
else
    log_error "Failed to extract the dataset. Please ensure you have the necessary permissions and disk space."
    exit 1
fi

# Step 3: Verify extraction
if [[ -d "$EXTRACT_DIR" ]]; then
    log_message "Dataset is ready for use. Extracted to: $EXTRACT_DIR"
else
    log_error "Extraction failed. The expected directory '$EXTRACT_DIR' was not found."
    exit 1
fi

# Step 4: Cleanup (optional)
log_message "Cleaning up temporary files..."
if rm -f "$DATASET_ARCHIVE"; then
    log_message "Temporary files removed successfully."
else
    log_error "Failed to remove temporary files. You may need to delete them manually."
fi

# Move files under unity/UnityGaussianSplatting/package under unity/package
mv unity/UnityGaussianSplatting/package unity/package

# End of script
log_message "Installation completed successfully."
