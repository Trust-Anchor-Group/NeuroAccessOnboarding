NeuroAccessOnboarding
=========================

This repository contains a service that facilitates onboarding of simple Neuro-Access digital identities. During the onboarding process, the user
validates its e-mail address and phone number with an onboarding Neuron, who directs the app to the most suitable host for the Neuro-Access account.
If the user chooses to create a simple Neuro-Access digital identity (i.e. only containing the phone number and e-mail address provided) the
digital identity can be automatically approved, if the host Neuron is able to validate the information with the onboarding Neuron. This repository
contains a serivce that performs this task: It registers an *Identity Authenticator*, which authenticates such simple Neuro-Access digital
identities with the onboarding Neuron, and approves the applications automatically, if the information matches the information validated during
the onboarding process.

## Projects

The solution contains the following C# projects:

| Project                    | Framework         | Description |
|:---------------------------|:------------------|:------------|
| `TAG.Identity.NeuroAccess` | .NET Standard 2.1 | Service module for the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md), that authenticates Neuro-Access identity applications with the onboarding Neuron. |

## Nugets

The following nugets external are used. They faciliate common programming tasks, and
enables the libraries to be hosted on an [IoT Gateway](https://github.com/PeterWaher/IoTGateway).
This includes hosting the bridge on the [TAG Neuron](https://lab.tagroot.io/Documentation/Index.md).
They can also be used standalone.

| Nuget                                                                                              | Description |
|:---------------------------------------------------------------------------------------------------|:------------|
| [Paiwise](https://www.nuget.org/packages/Paiwise)                                                  | Contains services for integration of financial services into Neurons. |
| [Waher.Content](https://www.nuget.org/packages/Waher.Content/)                                     | Defines an architecture for working with Internet Content. |
| [Waher.Events](https://www.nuget.org/packages/Waher.Events/)                                       | An extensible architecture for event logging in the application. |
| [Waher.IoTGateway](https://www.nuget.org/packages/Waher.IoTGateway/)                               | Contains the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) hosting environment. |
| [Waher.Persistence](https://www.nuget.org/packages/Waher.Persistence/)                             | Abstraction layer for object databases. |
| [Waher.Runtime.Inventory](https://www.nuget.org/packages/Waher.Runtime.Inventory/)                 | Maintains an inventory of type definitions in the runtime environment, and permits easy instantiation of suitable classes, and inversion of control (IoC). |
| [Waher.Runtime.Settings](https://www.nuget.org/packages/Waher.Runtime.Settings/)                   | Provides easy access to persistent settings. |

## Installable Package

The `TAG.Identity.NeuroAccess` project has been made into a package that can be downloaded and installed on any 
[TAG Neuron](https://lab.tagroot.io/Documentation/Index.md).
To create a package, that can be distributed or installed, you begin by creating a *manifest file*. The
`TAG.Identity.NeuroAccess` project has a manifest file called `TAG.Identity.NeuroAccess.manifest`. It defines the
assemblies and content files included in the package. You then use the `Waher.Utility.Install` and `Waher.Utility.Sign` command-line
tools in the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) repository, to create a package file and cryptographically
sign it for secure distribution across the Neuron network.

The Featured Peer-Reviewers service is published as a package on TAG Neurons. If your neuron is connected to this network, you can 
install the package using the following information:

| Package information                                                                                                              ||
|:-----------------|:---------------------------------------------------------------------------------------------------------------|
| Package          | `TAG.NeuroAccessOnboarding.package`                                                                            |
| Installation key | `eCcfYJJTV4r/SQWsYK2wo/2aHCBp+ZuvrdaUOeTp0Sa2oz5CuCqbteKkUoHX1XXeNSppMqY+49WA17bcceb2e763824b855eb832a996a598` |
| More Information | https://lab.tagroot.io/Community/Post/Neuro_Access_onboarding                                                  |

## Building, Compiling & Debugging

The repository assumes you have the [IoT Gateway](https://github.com/PeterWaher/IoTGateway) repository cloned in a folder called
`C:\My Projects\IoT Gateway`, and that this repository is placed in `C:\My Projects\NeuroAccessOnboarding`. You can place the
repositories in different folders, but you need to update the build events accordingly. To run the application, you select the
`TAG.Identity.NeuroAccess` project as your startup project. It will execute the console version of the
[IoT Gateway](https://github.com/PeterWaher/IoTGateway), and make sure the compiled files of the `NeuroAccessOnboarding` 
solution is run with it.

### Gateway.config

To simplify development, once the project is cloned, add a `FileFolder` reference
to your repository folder in your [gateway.config file](https://lab.tagroot.io/Documentation/IoTGateway/GatewayConfig.md). 
This allows you to test and run your changes to Markdown and Javascript immediately, 
without having to synchronize the folder contents with an external 
host, or recompile or go through the trouble of generating a distributable software 
package just for testing purposes. Changes you make in .NET can be applied in runtime
if you the *Hot Reload* permits, otherwise you need to recompile and re-run the
application again.

Example of how to point a web folder to your project folder:

```
<FileFolders>
  <FileFolder webFolder="/NeuroAccess" folderPath="C:\My Projects\NeuroAccessOnboarding\TAG.Identity.NeuroAccess\Root\NeuroAccess"/>
</FileFolders>
```

**Note**: Once file folder reference is added, you need to restart the IoT Gateway service for the change to take effect.

**Note 2**:  Once the gateway is restarted, the source for the files is in the new location. Any changes you make in the corresponding
`ProgramData` subfolder will have no effect on what you see via the browser.

**Note 3**: This file folder is only necessary on your developer machine, to give you real-time updates as you edit the files in your
developer folder. It is not necessary in a production environment, as the files are copied into the correct folders when the package 
is installed.
