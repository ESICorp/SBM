# Simple Batch Manager

***Project Description***

Nowadays batch applications are still a key piece of software in most organizations because a widely amount of business processes are amenable to batch processing. While online systems can also function when manual intervention is not desired, they are not typically optimized to perform high-volume, repetitive tasks. Therefore, even new systems usually contain one or more batch applications for updating information at the end of the day, generating reports, convert file formats, and other non-interactive tasks that must complete reliably within certain business deadlines.

Focusing on this concept, we designed and developed an application that can cover the most common needs in a easy way. Simple Batch Manager (SBM) is a simple yet powerful batch management solution for enterprises environments of any scale, from small organizations to mid-size or even large. Their mission is to be the backbone of all your batch process needs.

The solution aims to manage the execution of processes that do not require interaction with the user. To do this, a Windows service was created with the ability to act as a launch pad. The service contains a repository of special components that run on a SandBox in a timely manner. These special components can be developed to cover one or more specific functionalities, or you can also reuse the already included generic ones.

Generally speaking, the product comes ready as an out of the box solution, which means that after a successful install, some generic functionality is already included and ready to use with minimal configuration. Of course, it depends on the complexity of your tasks, but the generic components can be configured quickly, providing some common services such as a bulk mail service (send / receive), a basic ETL (extract, transform and load), a DB command executor, and finally a OS command executor.

In short, simply using the components included, you can for example: read a data source like an SQL DB and write an Excel XML report file with the basic ETL component, create a new folder in a specific place and copy the Excel file in it from the current location with the OS command executor, then send it via e-mail with the mail service component, etc.


***What is it and what is not***

SBM is not a framework or API of any kind. Is not a language extension or a new development paradigm. 

SBM is an application (Windows service), it is a complete solution to manage, execute, control, log and catalog your batch service needs. Is an integration software and a component oriented Middleware. You can develop any batch program for SBM using standard tools such as the C# language and the .Net Framework, following only some simple rules. You can also use the generic "ready to run" services already included. You must install the application with the installer taking into account all the prerequisites, and then you must configure it with your preferences. And that's it, you're ready to use SBM.


***Services Federation concept***

As a definition, each component that can be runned under SBM can be seeing as a service. This service should be specific, with a clear purpose, with low or limited complexity, always keeping the resources consumption as low as possible, and must have a maximum time interval for its execution. If your services are designed with this conception in mind, you can build a wide range of common service blocks that can collaborate with each other or serve many diverse application or systems. 

After successfully implements some services, you probably realize that a few could be widely used, transforming it into a common or generic service. A clear example of these generic components are the generic services already included with the solution. Thus, you soon have a collection of generic services and specialized services. These last one are services which implements certain tasks or operations that are specific or unique in your organization.

As more and more services are incorporated, their ecosystem of services becomes more complex. Because of this, you must prioritize the development of services that can be shared with many applications or systems, making it reusable. The more reusable your service is, the more applications or systems could take advantage of that service. Since this type of service has some specific behavior and / or uses specific resources and / or follows certain common business rules, it must be considered different from a generic service. From the organizational point of view, we call this type of service a "standard service".

This set of generic, standard and specialized services makes up the so-called Service Federation Concept. These Federation of Services can be consumed on demand within the organization, or scheduled to run at a specific date.

SBM was designed to extract the maximum potential of this concept of Services Federation, keeping a complete catalog of services and meeting all requirements of on demand execution or deferred execution. Having at the same time, the control and tracing mechanisms to follow every event and execution result of each dispatched service. 

***Project Documentation***

[https://github.com/andrescastiglia/SBM/wiki](https://github.com/andrescastiglia/SBM/wiki)
