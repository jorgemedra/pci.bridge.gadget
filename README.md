# Cisco Finesse: Compliance with PCI Standards
## 1 Overview
The objective of this solution is to carry out automated bank payment and refund transactions by credit card, transferring to IVR, using the security standards of the PCI (Payment Card Industry).

**Note:** _The CVP application for banking transactions is not included_

## 2 Architecture
In the screenshot below, the architecture of the project is shown.
![Architecture](https://github.com/souldev23/pci.bridge.gadget/blob/main/screenshoots/architecture.png?raw=true)

##### Architecture components
The architecture diagram shows all the components that participate in the solution. The functionality of each is described below.
* **Bridge Service:** It is the component that is responsible for communication between all components. It receives data from the CRM and functions how a Webhook, after passes it to Finesse and sends the notification received from the IVR to the CRM.
* **Finesse Gadget:** Check bridge data and then fill out the CallVars and make the transfer to the IVR.
* **IVR (code no included):**: It collects the client's data so that through the consumption of Web Services, it can carry out the sale or refund banking transactions. In the end, it notifies the Bridge Service of the result of the transaction.

## 3 Description
1. The CRM triggers the payment or refund process (as the case may be).
1. The data is sent to the Bridge through an HTTP POST request.
1. The Gadget obtains this data through a GET request.
1. Before transferring the gadget, fill the callVars with the information provided by the CRM.
1. The Gadget makes a blind transfer to the IVR and notifies the Bridge.
1. The IVR collects the customer's credit card data and obtains the complementary data through an API, to make the payment.
1. Once the transaction is completed through a payment API, the IVR notifies the Bridge through an HTTP POST request, sent using the Cisco cell, consuming web services.
1. The bridge service returns the response to the CRM.  

## 4 Basic concepts
The repository has two projects:
* Finesse_Bridge
* GadgetPCI

#### Finesse_Bridge
The Bridge is developed in C # language and uses the HttpListener class. The class belonging to the System.Net library, which allows you to emulate an IIS Web site. The specifications with development dependencies are listed below.
*	Visual Studio 2015 project
* .Net Framework: 4.5v
*	References:

![References](https://github.com/souldev23/pci.bridge.gadget/blob/main/screenshoots/Requirements.png?raw=true)

The content of the solution is shown below, and notable files are marked in red.
![Solution content](https://github.com/souldev23/pci.bridge.gadget/blob/main/screenshoots/Finesse_Bridge%20Solution.png?raw=true)

The project is customizable according to the requirements of the client, and this configuration is found in the files:
* **App.config:** This file contains all the settings the service depends on to function the way you want it to. There are 4 configuration parameters for the Bridge application, below is described what each of them refers to.
  * **ResponseTime:** It refers to the maximum waiting time that must elapse since the CRM request arrives and until the IVR notifies the result of the bank transaction. The time is given in seconds.
  * **AfterEndIVRTime:** These are the seconds it will take to clear the Gadget data, as well as the Bridge, once the notification of the call ended sent by the IVR has been received.
  * **NotReadyTime:** It is the maximum waiting time since the Finesse Gadget notifies that the call has been transferred to the IVR, and if no other notification is received, it is taken as a missed call (this measure was taken since the transfer is to blind).
  * **Protocol:** Indicates whether the bridge will work under an open environment that is secure, or closed and insecure, since the certificate will not be used, which is created during the installation of the service.

* **Log4net.config:** Contains the configuration of the logging library, in this file you can change how the logs will be created and saved.

**Note:** _To use the service in debug mode, you need to run VS 2015 as administrator_

#### GadgetPCI
The Gadget is a component that is embedded within the Cisco Finesse interface, whose main task is to take the bridge data, store it in the call variables and transfer the call to the IVR to complete the transaction.

It's important to mention, this gadget was designed for Finesse v11.5

The gadget project contains the following files:
* **GadgetPCI.xml:** Contains the gadget view.
* **GadgetPCI.js:** Contains the Finesse logic.
* **LogicPCI.js:** Control the gadget view.
* **ThemePCI.css:** Contains the styles that are used.

## GUI
Below is a screenshot of the GUI within Finesse

Sales GUI
![GadgetPCI Sales GUI](https://github.com/souldev23/pci.bridge.gadget/blob/main/screenshoots/Gadget%20vta.png?raw=true)

Refund GUI
![GadgetPCI Refund GUI](https://github.com/souldev23/pci.bridge.gadget/blob/main/screenshoots/Gadget%20dvl.png?raw=true)

The GUI shown is conditional on the number of parameters.

### TEST
HTML Test interface:
```HTML
<body>
    <fieldset id="fsSales">
        <table>
            <tr>
                <td>No. Orden:</td>
                <td><input id="nOrden" /></td>
            </tr>
            <tr>
                <td>Banco:</td>
                <td><input id="cBanco" /></td>
            </tr>
            <tr>
                <td>Monto:</td>
                <td><input id="nMonto" /></td>
            </tr>
            <tr>
                <td>Armado:</td>
                <td><input id="bArmado" type="checkbox" checked="checked" /></td>
            </tr>
            <tr>
                <td>Locación:</td>
                <td><input id="nLocation" /></td>
            </tr>
            <tr>
                <td>Parametro Cybersource:</td>
                <td><input id="cCyberS" /></td>
            </tr>
            <tr>
                <td><input value="Send Sale" type="button" onclick="sendSale();" /></td>
                <td></td>
                <td></td>
                <td></td>
            </tr>
            <tr>
                <td></td>
               <td></td>
            </tr>
        </table>
    </fieldset>
    <fieldset id="fsDevs">
        <table>
            <tr>
                <td>No. Orden:</td>
                <td><input id="nOrdenD" /></td>
            </tr>
            <tr>
               <td>Tarjeta truncada:</td>
                <td><input id="cCard" /></td>
            </tr>
            <tr>
                <td>Monto:</td>
                <td><input id="nMontoD" /></td>
            </tr>
            <tr>
                <td><input value="Send Refund" type="button" onclick="sendRefund();" /></td>
                <td></td>
                <td></td>
            </tr>
            <tr>
                <td></td>
                <td></td>
            </tr>
        </table>
    </fieldset>
<body>
```
And the Javascript code behind sendSale and sendRefund look's like:
```javascript
function sendSale() {
    var bArm = document.getElementById("bArmado");

    var dataClient = {  
        "arm":bArm.checked,
        "nor":$("#nOrden").val(),
        "bnc":$("#cBanco").val(),
        "mnt": $("#nMonto").val(),
        "loc": $("#nLocation").val(),
        "pcs": $("#cCyberS").val()
    }
    console.log(JSON.stringify(dataClient));
    $.ajax({
        url: "http://127.0.0.1:8880/pci_bridge/sls/",
        type: "POST",
        data: JSON.stringify(dataClient),
        success: handleResponseSuccess,//Your function
        error: handleResponseError//Your function
    });
}

function sendRefund() {
    var bArm = document.getElementById("bArmado");

    var dataClient = {
       "nor": $("#nOrdenD").val(),
        "tct": $("#cCard").val(),
        "mnt": $("#nMontoD").val()
    }
    console.log(JSON.stringify(dataClient));
    $.ajax({
        url: "http://127.0.0.1:8880/pci_bridge/dvl/",
        type: "POST",
        data: JSON.stringify(dataClient),
        success: handleResponseSuccess,
        error: handleResponseError
    });
}
```
Finally, the notification for local test that should come from the IVR should have the following format:
```javascript
function Notify() {
    var dataClient = {
        "nor": "738389",
        "tct": "4765********9031",
        "mnt": 83773.23,
        "mne": "SUCCESS",
        "fec": "20191030",
        "nau": "5578765443",
        "pro": "3msi",
        "hor": "12:20:35"
    }
    console.log(JSON.stringify(dataClient));
    $.ajax({
        url: "http://127.0.0.1:8880/pci_bridge/ivr/",
        type: "POST",
        data: JSON.stringify(dataClient),
        success: handleResponseSuccess,
        error: handleResponseError
    });
}
```
Enjoy.
