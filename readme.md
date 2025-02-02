# Environment Monitor

Purpose of the solution is to display measurement data, humidity + temperature for now, from different devices.

This solution doesn't include the part of generating the measurements. The solution does, however, include an Azure function the purpose of which is to listen to Azure IoT Hub and store measurement data to a DB. 


## Projects

### EnvironmentMonitor.Domain

Contains model / entity classes. Has no project dependencies.

Contains interfaces for common services that are implemented in the Infrastructure project.

### EnvironmentMonitor.Application

"Application layer". Includes AutoMapper and is meant to be used by "driving applications", such as the WebApi / Azure function. 

### EnvironmentMonitor.Infrastructure

Contains the implementation for interfaces declared in the Domain project. Includes "infra"-functionality, such as:

- EF core DB configuration
- Repository implementations
- .NET identity logic.

Services of the infrastructure project shouldn't be directly used but rather used by services in the Application project.

### EnvironmentMonitor.WebApi

WebApi project. Includes the end points for fetching measurement data.

### EnvironmentMonitor.HubObserver

Azure Function project which includes a function for listening to IoT hub. The function listens to messages arriving to the IoT hub, processes them and saves them to a DB.


