# Cisco Finesse: Compliance with PCI Standards
## 1 Overview
The objective of this solution is to run automated bank payment or refund transactions by credit card, transferring to CVP and using the PCI's security standards (Payment Card Industry).

**Note:** _The CVP application for banking transactions is not included_

## 2 Architecture
In the screenshot below, the architecture of the project is shown.
![Architecture](https://github.com/souldev23/pci.bridge.gadget/blob/main/screenshoots/architecture.png?raw=true)

##### Architecture components
The architecture diagram shows all the components that participate in the solution. The functionality of each element is described below.

* **Bridge Service:** *Windows Serviced developed in C#*. It is the component that has the responsibility to provide a Webhook port for receive all the notifications that the CVP App needs to dispatch, in Real-Time, to the Gadget and CRM.
* **Finesse Gadget:** Component embedded in Finness session. its functions are: 
Gathering call data to perform a transaction.
Transferring the call to the CVP to perform a transaction, attaching the data into the *CallVariables*, including the URL that the Bridge Service provides to receive the notification.
Check for the result of the transaction, Asking the Bridge for the result of the transaction.
* **CVP (code not included)**: It receives the Client's call and data, performs the transaction. At the end of the transaction, the CVP App reports the result to the Webhook, implemented by the Bridge Service, sending a JSON Document. 

## 3 Description

1. A call arrives at the Agent's extension and Finnesse session.
2.The Gadget receives the call's data.
3. After a while, the Agent triggers a process of payment or refund, using the CRM.
4. The CRM sends the data to the Bridge through an HTTP POST request, using its Webhook interface, which is a unique port but different URL to attend the Gadget and another one to the CRM.
5. Due to the Gadget requires monitoring for new commands, it's asking for new commands/events to the bridge, sending HTTP GET Requests. At that moment, the Gadget fetches for the information to be added to the call at the moment of transferring it.
6. Using the Gadget, the call is transferred, using a Blind transfer, adding the CallsVars to the consult call. At this moment, the Gadget is waiting for the feedback of the transaction in the CVP.
7. The CVP runs the transaction and, at the end of it, reports the result to the Webhook of the Bridge Service, sending an HTTP Post Request, using the Cisco cell.
8. Using the same process for monitoring the Bridge, the Gadget gets the Transaction's result and shows the feedback to the Agent, showing the result in a field of the Gadget. 

## 4 Basic concepts
The repository has two projects:
* Finesse_Bridge
* GadgetPCI

#### Finesse_Bridge
The Bridge is developed in C # language and uses the HttpListener class. The class belonging to the System.Net library, which allows you to emulate an IIS Web site. The specifications with development dependencies are listed below.
* Visual Studio 2015 project
* .Net Framework: 4.5v
* References:

![References](https://github.com/souldev23/pci.bridge.gadget/blob/main/screenshoots/Requirements.png?raw=true)

The content of the solution is shown below, and notable files are marked in red.
![Solution content](https://github.com/souldev23/pci.bridge.gadget/blob/main/screenshoots/Finesse_Bridge%20Solution.png?raw=true)

The project can be configured according to the requirements of the client, and this configuration is found in the files:
* **App.config:** This file contains all the settings the service requires to build it. There are 4 configuration parameters for the Bridge application that need to look after:
  * **ResponseTime:** The maximum waiting time in seconds that must elapse since the CRM request arrives and until the CVP notifies the result of the transaction. The time is set in seconds.
  * **AfterEndIVRTime:** The time in seconds that it will take to clear the Gadget data, as well as the Bridge, once the End Call's notification has been received.
  * **NotReadyTime:** The maximum waiting time in seconds since the Finesse Gadget notifies that the call has been transferred to the IVR, and if no other notification is received, it will take it (Call) as a missed call (this measure will be taken since the blind transfer is performed).
  * **Protocol:** Set HTTP or HTTPS protocol.

* **Log4net.config:** Contains the configuration of the logging library.

> **Note:** To use the service in debug mode, you need to run VS 2015 as administrator.

#### GadgetPCI
The Gadget, **designed for Finesse v11.5**, is a component that is embedded in the Cisco Finesse interface, which main task is to take the bridge data, store it in the call variables and transfer the call to the IVR to complete the transaction.

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
