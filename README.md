# Cisco Finesse: Compliance with PCI Standards
## 1 Overview
The objective of this solution is to carry out automated bank payment and refund transactions by credit card, transferring to IVR, using the security standards of the PCI (Payment Card Industry).

## 2 Architecture
In the screenshot below, the architecture of the project is shown.
![Architecture](https://github.com/souldev23/Oficced.Bridge.Gadget/blob/master/screenshoots/architecture.png?raw=true):

## 3 Description
1. The CRM triggers the payment or refund process (as the case may be).
1. The data is sent to the Bridge through an HTTP POST request.
1. The Gadget obtains this data through a GET request.
1. Before transferring the gadget, fill the callVars with the information provided by the CRM.
1. The Gadget makes a blind transfer to the IVR and notifies the Bridge.
1. The IVR collects the customer's credit card data and obtains the complementary data through an API, to make the payment.
1. Once the transaction is completed through a payment API, the IVR notifies the Bridge through an HTTP POST request, sent using the Cisco cell consuming web services.
1. The bridge service returns the response to the CRM.  

## 4 Basic concepts
The repository has two projects:
* Finesse_Bridge
* GadgetPCI

#### Finesse_Bridge
The Bridge is developed in C # language and uses the HttpListener class. The class belonging to the System.Net library, which allows you to emulate an IIS Web site. The specifications with development dependencies are listed below.
*	Visual Studio 2015 project
* .Net Framework: Version 4.5
*	References:

![References](https://github.com/souldev23/Oficced.Bridge.Gadget/blob/master/screenshoots/Requirements.png?raw=true):

The project is customizable according to the requirements of the client, and this configuration is found in the files:
* **App.config:** This file contains all the settings the service depends on to function the way you want it to. There are 4 configuration parameters for the Bridge application, below, it is described what each of them refers to.
  * **ResponseTime:** It refers to the maximum waiting time that must elapse since the CRM request arrives, and until the IVR notifies the result of the bank transaction. The time is given in seconds.
  * **AfterEndIVRTime:** These are the seconds it will take to clear the Gadget data, as well as the Bridge, once the notification of the call ended sent by the IVR has been received.
  * **NotReadyTime:** It is the maximum waiting time since the Finesse Gadget notifies that the call has been transferred to the IVR, and if no other notification is received, it is taken as a missed call (this measure was taken since the transfer is to blind).
  * **Protocol:** Indicates whether the bridge will work under an open environment that is secure, or closed and insecure, since the certificate will not be used, which is created during the installation of the service.

* **Log4net.config:** Contains the configuration of the logging library, in this file you can change the way in which the logs will be created and saved.
#### GadgetPCI
The Gadget is a component that is embedded within the Cisco Finesse interface, whose main task is to take the bridge data, store it in the call variables and transfer the call to the IVR to complete the transaction.

The gadget project contains the following files:
* **GadgetPCI.xml:** Contains the gadget view.
* **GadgetPCI.js:** Contains the Finesse logic.
* **LogicPCI.js:** Control the gadget view.
* **ThemePCI.css:** Contains the styles that are used.
