# 🖥️ Process Control System

This project is a **process control system** that consists of a **Server** and a **Task Manager**, which communicate via **TCP and UDP sockets** to monitor processor and memory usage.

## 📜 Overview of the Project

### **🖥️ Server Component**

-    **Manages system processes** and monitors **CPU & Memory Usage** of currently running tasks.
-    **Handles automatic client connections** and updates the **process state**.
-    Uses **TCP for reliable communication** and **UDP for initial client discovery**.
----------
### **🖥️ Task Manager Component**
-    **Displays currently active processes** running on the Server.
-    **Provides a stylish interface for process monitoring and management**.
-    Uses **UDP for communication with the Server**.
-    **Automatically starts**, connects to the **Server**, and **retrieves system state**.
-    **Provides real-time process tracking**.
-    **Automatically closes** if the Server is closed to prevent unwanted errors.
    
----------
### **🖥️ Client Component**
-    **Includes functionality for automatic process generation**.
-    Uses **TCP for reliable communication** and **UDP for discovery**.
-    **Ensures connection**, with increasing timeouts between retries.
-    **Automatically closes** if the Server is closed to prevent unwanted errors.
    
----------

### **📡 Communication Flow**

1️⃣ **Client sends a registration request via UDP**  
2️⃣ **Server responds with a TCP port assignment**  
3️⃣ **Client establishes a TCP connection to the Server**  
4️⃣ **The communication ensues**  
