# **File Monitoring Application**  

This application **monitors a local folder** and automatically **uploads & deletes files** to **MOVEit Transfer** via its **REST API**.

## **Features**  
✅ Monitors a specified folder for new & deleted files  
✅ Securely uploads files to MOVEit Transfer  
✅ Configurable via `appsettings.json`  

---

## **Setup Instructions**  

### **1️⃣ Configure the Application**  
Edit the configuration file at:  
📂 **`src/appsettings.json`**  

#### **Update the following settings:**
- **`FileTransfer.BaseUrl`** – Set this to your **MOVEit API URL**.
- **`FileTransfer.Auth.PasswordCredentials`** – Enter your **MOVEit username** and **password**.  
- **`Monitor.FolderPath`** – Set the path to the local directory you want to monitor.  

### **2️⃣ Run the Application**  
Once configured, start the application to begin monitoring and uploading files.  

---

## **Example Configuration (`appsettings.json`)**  
```
{
  "FileTransfer": {
    "BaseUrl": "https://moveitcloud.com/api/v1",
    "Auth": {
      "GrantType": "password",
      "PasswordCredentials": {
        "Username": "your-username",
        "Password": "your-password"
      }
    }
  },
  "Monitor": {
    "FolderPath": "C:\\Path\\To\\Monitor",
    "DelayBetweenScansInSeconds": "1"
  }
}
```

## What's Next

While the application is fully functional, there are areas targeted for improvement:

- **Unit Testing:** Adding tests to cover core functionalities.
- **Retry Mechanisms:** Introducing a policy to gracefully handle transient http faults.
- **Security Enhancements:** Improving password handling and storage.

These improvements were deprioritized due to time constraints and are slated for future development.
