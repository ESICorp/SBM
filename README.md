#Simple Bath Manager

##Project Description

Nowadays batch applications are still a key piece of software in most organizations because a widely amount of business processes are amenable to batch processing. While online systems can also function when manual intervention is not desired, they are not typically optimized to perform high-volume, repetitive tasks. Therefore, even new systems usually contain one or more batch applications for updating information at the end of the day, generate reports, convert file formats, general asynchronous processing, and other non-interactive tasks that must complete reliably within certain business deadlines.

Focusing on this concept, we designed and developed an application that can cover the most common needs in an easy way. Simple Batch Manager (**SBM**) is a simple yet powerful batch management solution for enterprises environments of any scale, from small organizations to mid-size or even large. Their mission is to be the backbone of all your batch process needs.

The solution aims to manage the execution of processes that do not require interaction with the user. To do this, a Windows service was created with the ability to act as a launch pad. The service contains a repository of special components that run on a SandBox in a timely manner. These special components can be developed to cover one or more specific functionalities, or you can also reuse the already included generic ones.

Generally speaking, the product comes ready as an out of the box solution, which means that after a successful install, some generic functionality is already included and ready to use with minimal configuration. Of course, it depends on the complexity of your tasks, but the generic components can be configured quickly, providing some common services such as a _bulk mail service_ (send / receive), a _basic ETL_ (extract, transform and load), a _DB command executor_, and finally an _OS command executor_.

In short, just using the components included, you can for example: read a data source like an SQL DB and then write an Excel XML report file with the basic ETL component, create a new folder in a specific place and copy such Excel file on it from the current location with the OS command executor, then send it via e-mail with the mail service component, etc.



#What is it and what is not

**SBM** is not a framework or API of any kind. Is not a language extension or a new development paradigm. 

**SBM** is an application (Windows service), it is a complete solution to manage, execute, control, log and catalog your batch service needs. Is an integration software and a component oriented Middleware. You can develop any batch program for **SBM** using standard tools such as the C# or VB language (like others) and the .Net Framework, following only some simple rules. You can also use the generic "ready to run" services already included. You should install the application with the installer taking into account all the prerequisites, and then you can configure it with your preferences. And that's it, you're ready to use **SBM**.


***Project Documentation***

[https://github.com/ESICorp/SBM/wiki](https://github.com/ESICorp/SBM/wiki)
