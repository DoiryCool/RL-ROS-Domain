# RL-ROS-Domain(Developing)

---
![替代文本](./Images/m1-2.png)
This is a project about reinforcement learning on Unity Agent and ROS. Here, I have provided some scenes for reference.

## Enviroment

---

| Component | Version      | Remark                                                              |
| --------- | ------------ | ------------------------------------------------------------------- |
| System    | Ubuntu 22.04 |                                                                     |
| Unity Hub | 2022.3.6f1   |                                                                     |
| Python    | 3.10.8       | 3.10.x                                                              |
| Pytorch   | 1.11.0+cu113 | For the CUDA                                                        |
| ml-agents | Release 20   | [Github Link](https://github.com/Unity-Technologies/ml-agents)         |
| CUDA      | 11.3         | [CUDA Link](https://developer.nvidia.com/cuda-11.3.0-download-archive) |
| ROS2      | Humble       |                                                                     |

## Demo

---

|   | Scene       | Image                                                     | Description                                                                                                                                                                                                                                                                                                                                              |
| - | ----------- | --------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1 | RollerAgent | ![替代文本](./Images/m1-1.png)  | This scenario describes an intelligent agent with "satiety" and "energy" attributes. The agent uses multiple rays to perceive its environment. The "satiety" decreases over time, and movement consumes "energy." Yet, if not hungry, the agent's "energy" replenishes gradually. The agent can also consume "Target" entities to restore its "satiety." |
| 2 | RoboticCar  | ![替代文本](./Images/m2-1.png)                              | In this scenario, you will have access to a vehicle equipped with the capability to perform various maneuvers such as moving forward and making turns. The objective is to train the vehicle to successfully accomplish specific tasks.                                                                                                                  |

## SetUp

---

#### 1.Install ml-agents

You can follow this [official doc](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Installation.md) or follow me to install in Ubuntu 22.04(Since you may have problem in installing here).

##### 1.1 Create Conda Enviroment

```bash
conda create -n ml-agment python=3.10.8
```

##### 1.2 Install Pytorch

```bash
conda activate ml-agents
pip3 install torch==1.11.0+cu113 torchvision==0.12.0+cu113 torchaudio==0.11.0 --extra-index-url https://download.pytorch.org/whl/cu113
```

##### 1.3 Clone The Ml-Agents Release(Recommended)

```bash
mkdir -p ~/Projects && cd ~/Projects
git clone --branch release_20 https://github.com/Unity-Technologies/ml-agents.git
```

##### 1.4 Install Python Libraries

```bash
cd ~/Projects
pip3 install -e ./ml-agents-envs
pip3 install -e ./ml-agents
```

#### 2.Load ml-agents to  Unity Project
You can follow the [Offical Document](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Getting-Started.md).

#### 3.Install CUDA(Ubuntu 22.04)
##### 3.1 Install Nvidia-Driver.
```bash
sudo apt update && sudo apt upgrade -y
# or sudo apt update
ubuntu-drivers devices
sudo ubuntu-drivers autoinstall
# or sudo apt install nvidia-driver-535
reboot
nvidia-smi
```
##### 3.2 Install CUDA 11.3
```bash
wget https://developer.download.nvidia.com/compute/cuda/11.3.0/local_installers/cuda-repo-ubuntu2004-11-3-local_11.3.0-465.19.01-1_amd64.deb
sudo dpkg -i cuda-repo-ubuntu2004-11-3-local_11.3.0-465.19.01-1_amd64.deb
sudo apt-key add /var/cuda-repo-ubuntu2004-11-3-local/7fa2af80.pub
sudo apt-get update
sudo apt-get -y install cuda-11-3

```
##### 3.3 Set Enviroment Variables
```bash
export PATH=$PATH:/usr/local/cuda/bin  
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/cuda/lib64
```
## License

---

[Apache License 2.0](./LICENSE)
