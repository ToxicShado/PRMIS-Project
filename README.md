# 🖥️ Process Control System

This project is a **process control system** that consists of a **Server** and a **Task Manager**, which communicate via **TCP and UDP sockets** to monitor processor and memory usage.

## 📜 Overview of the Project

### **🖥️ Server Component**

-    **Manages system processes** and monitors **CPU & Memory Usage** of currently running tasks.
-    **Handles automatic client connections** and updates the **process state**.
-    Uses **TCP for reliable communication** and **UDP for initial client discovery**.
-    **Provides real-time process tracking**.
----------
### **🖥️ Task Manager Component**
-    **Displays currently active processes** running on the Server.
-    **Provides a user-friendly interface for process monitoring and management**.
-    Uses **TCP for communication with the Server**.
    
----------
### **🖥️ Client Component**
-    **Automatically starts**, connects to the **Server**, and **retrieves system state**.
-    **Includes functionality for automatic process generation**.
-    Uses **TCP for reliable communication** and **UDP for discovery**.
    

----------

### **📡 Communication Flow**

1️⃣ **Client sends a registration request via UDP**  
2️⃣ **Server responds with a TCP port assignment**  
3️⃣ **Client establishes a TCP connection to the Server**  
4️⃣ **Server continuously sends system stats & active processes**  

## 🚀 Features
✅ **UDP-based Client Registration** is used to obtain a dynamic TCP port.  
✅ **Task Manager**: A stylish interface to track the current state of the Server. 
✅ **Process Monitoring**: Tracks processor & memory usage.  
✅ **Graceful Shutdown**: Server exits when Task Manager closes.
✅ **Many [STATUS] and [INFO] updates** so that you may know *exactly* what is happening at any moment in time.  
✅ **Error Handling** for connection failures & automatic retries.  ## 📜 **Project Overview**

This project is a **multi-process control system** that monitors **CPU and memory usage** and manages active processes through a **client-server architecture**.
