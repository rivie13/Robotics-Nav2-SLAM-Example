#!/bin/bash

set -xe

colcon build
source install/setup.bash

exit 0

