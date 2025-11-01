# DOT NET PROJECT
TEAM:
1) Rebeca Cojocarescu
2) Andrei Mardare
3) Cosmin Mazilu

# Docker Postgres

### For Windows

1.  **Install Docker Desktop**
    * Download and install Docker Desktop from the [official website](https://www.docker.com/products/docker-desktop/).

2.  **Run the Application**
    * Open a terminal (like PowerShell or Command Prompt).
    * Navigate to the root directory of this project (where the `docker-compose.yml` file is located).
    * Run the following command to build the images and start the containers in detached mode (in the background):
        ```bash
        docker-compose up -d
        ```

3.  **Stop the Application**
    * To stop and remove the containers, networks, and volumes, run the following command from the same directory:
        ```bash
        docker-compose down
        ```
---

### 🐧 For Linux

1.  **Install Docker Engine**
    * Follow the official installation guide for your specific Linux distribution (e.g., Ubuntu, Fedora, CentOS):
    * [https://docs.docker.com/engine/install/](https://docs.docker.com/engine/install/)

2.  **Install `docker-compose`**
    * Follow the official guide to install the standalone `docker-compose` binary:
    * [https://docs.docker.com/compose/install/standalone/](https://docs.docker.com/compose/install/standalone/)

3.  **Add User to `docker` Group (Recommended)**
    * To run Docker commands without needing `sudo` every time, add your user to the `docker` group:
        ```bash
        sudo usermod -aG docker $USER
        ```
    * **Important:** You must **log out and log back in** for this group change to take effect.

4.  **Run the Application**
    * Open a terminal.
    * Navigate to the root directory of this project (where the `docker-compose.yml` file is located).
    * Run the following command to build and start the containers in detached mode:
        ```bash
        docker-compose up -d
        ```

5.  **Stop the Application**
    * To stop and remove the containers, networks, and volumes, run the following command from the same directory:
        ```bash
        docker-compose down
        ```

