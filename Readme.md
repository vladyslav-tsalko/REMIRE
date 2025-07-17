# 🩺 REMIRE (REhabilitation in MIxed REality)

## 📖 Overview
REMIRE is a **Mixed Reality platform** designed to support upper-limb rehabilitation for stroke patients by combining real-world interactions with virtual tasks. It builds on the original REVIRE system but transitions from fully virtual environments to mixed reality, enabling users to perform tasks directly on physical surfaces like tables.

By using advanced hand tracking and real-world surface detection, REMIRE creates a more **intuitive**, **realistic**, and **engaging** rehabilitation experience. Developed for the **Meta Quest 3** headset, it supports both clinical and home-based therapy scenarios.

This project was developed as part of my **bachelor thesis** at the University of Vienna in collaboration with the Research Group Neuroinformatics.

---

## 📺 Video Demo

Watch a recorded demonstration of REMIRE in action:

👉 [Demo Video (YouTube)](https://youtu.be/eGXODrJl9Fo)

This video showcases:
- Mixed reality environment setup
- Scene understanding and table detection
- Hand tracking and natural object manipulation
- Adaptive difficulty based on user reach

---

## 🎮 Key Features

**Mixed Reality Environment**
- Anchors virtual rehabilitation tasks to real tables using Meta’s [Passthrough and Scene Understanding APIs 🌐](https://developers.meta.com/horizon/documentation/unity/mr-experience-and-use-cases)

**Adaptive Task Calibration**
- Automatically calibrates tasks to match the user's reach and table size.

**Advanced Hand Tracking**
- Full skeletal hand tracking using Meta XR Hands API.
- Natural interaction with UI and objects (pinch-to-select, grab, release).

**Realistic Object Interaction (GrabRules System)**
- Designed and implemented a fully modular **GrabRules system** for object interactions.
- Unlike the original REVIRE, where grabbing logic was hand-centric and rigid, REMIRE shifts responsibility to each object:
  - Each object independently determines if it’s being grabbed based on **user hand pose and finger combinations**.
  - Supports **single-hand and two-hand interactions** seamlessly.
- Enables **dynamic finger switching** without dropping objects (e.g., transitioning from thumb+index to thumb+middle mid-grab).
- Includes **safety features** like:
  - Preventing objects from spawning inside the user’s hand.
  - Adjusting object placement to avoid table edges.
- This system creates **highly natural and responsive interactions**, critical for motor rehabilitation where users may have limited precision or strength.

**Modular Task System**
- Unified architecture for easy expansion of rehabilitation exercises.
- Tasks adapt dynamically to different levels of difficulty and user needs.

**Optimized for Performance**
- Reduced polygon counts of key 3D models for smooth rendering on Meta Quest 3.
- Refactored architecture improves scalability and maintainability.

**User-Centered Design**
- UI follows user movement without being obtrusive.
- Feedback mechanisms for calibration and task progression.

**Documentation**
- 📖 [Bachelor Thesis Report (PDF)](docs/REMIRE-Thesis.pdf)

---

## 🛠 Technologies Used
- Unity **6000.0.41f1 LTS**
- Meta XR SDK (v74)
- Mixed Reality Utility Kit (MRUK)
- Meta Quest 3 (target platform)
- C# (Unity scripts)
- JetBrains Rider (IDE)
- Git (version control)

---

## 🚀 How to Run

### 🖥 PC Setup
1. Download and install the **Meta Quest Developer Hub (MQDH)**:
   - [Windows](https://developers.meta.com/horizon/downloads/package/oculus-developer-hub-win/)
   - [Mac OS](https://developers.meta.com/horizon/downloads/package/oculus-developer-hub-mac/)

2. Ensure Unity version **6000.0.41f1** is installed.

### 👤 Meta Account Setup
- Use the **primary developer account** on your Meta Quest device.
- Enable **Developer Mode** via the Horizon app: https://horizon.meta.com/

### 📱 Mobile Device Setup
1. Download the Horizon app.
2. Log in with your Meta account.
3. Connect your Meta Quest 3 and enable Developer Mode.

### 🎮 Meta Quest Device Setup
1. Connect your Meta Quest 3 to your PC via USB cable.
2. Allow the connection on the device when prompted.
3. Enable the following features:
   - Environment Tracking
   - Passthrough
   - Hand Tracking

### ⚠ Troubleshooting "Unauthorized" in MQDH
If MQDH shows **unauthorized**:
- Recheck that Developer Mode is enabled (via the Horizon app).
- Unplug and replug the USB cable.
- Toggle Developer Mode off and on again.
- Reboot the device.

### ⚡ Build & Deploy
1. Clone this repository.
2. Open the project in Unity Hub and set the target platform to **Android**.
3. Build and deploy the APK to your Meta Quest 3 using MQDH or `adb install`.

---

## 📝 What I Learned
- Designing and implementing a modular architecture for mixed reality apps.
- Optimizing hand-object interactions with custom grab rules.
- Integrating Meta XR SDK for advanced hand tracking and scene understanding.
- Refactoring legacy VR codebases (REVIRE) for MR scalability.
- Balancing performance and visual fidelity on standalone XR hardware.

---

## 🏥 Project Context
This project was developed as my **bachelor thesis** at the University of Vienna under the supervision of **Univ.-Prof. Moritz Grosse-Wentrup**. It extends the work on REVIRE (VR rehabilitation) into the mixed reality domain using modern hardware and software tools.

The goal was to demonstrate how mixed reality can enhance motor rehabilitation for stroke patients by providing a more natural and user-friendly training environment.

---

## 📚 References
- Mihic Zidar, L., et al. (2024). *REVIRE: A Virtual Reality Platform for BCI-Based Motor Rehabilitation.*
- Meta Developers: Unity MR Development Guide
- Unity MR Utility Kit (MRUK) Documentation

---

