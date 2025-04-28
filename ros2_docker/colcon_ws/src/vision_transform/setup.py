
from setuptools import find_packages, setup

package_name = 'vision_transform'

setup(
    name=package_name,
    version='0.0.0',
    packages=find_packages(exclude=['test']),
    data_files=[
        ('share/ament_index/resource_index/packages', ['resource/' + package_name]),
        ('share/' + package_name, ['package.xml']),
    ],
    install_requires=['setuptools'],
    zip_safe=True,
    maintainer='jaych',
    maintainer_email='jasenhow@gmail.com',
    description='YOLO-based vision transform for ROS 2',
    license='Apache 2.0',  # Update as needed
    tests_require=['pytest'],
    entry_points={
        'console_scripts': [
            'yolo_node = vision_transform.yolo_node:main',
            'multirobot_yolo = vision_transform.multirobot_yolo:main',
        ],
    },
)

