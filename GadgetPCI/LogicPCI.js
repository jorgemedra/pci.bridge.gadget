var onCall = false;

function sendGet() {
    console.log("=========== Send? " + send + " ==============");
    if(send)
    {
        $.ajax({
            url: "http://localhost:8880/pci_bridge/get-di/",
            type: "GET",
            success: handleResponseS,
            error: handleResponseE
        });
    }
}

function putCID(cid) {
    console.log("CID: " + cid);
    var dataClient = {
        "cid": cid
    }
    console.log("CID JSON: " + JSON.stringify(dataClient));
    console.log(JSON.stringify(dataClient));
    $.ajax({
        url: "http://127.0.0.1:8880/pci_bridge/dialog/",
        type: "POST",
        data: JSON.stringify(dataClient),
        success: handleResponseS,
        error: handleResponseE
    });
}


function sendClear() {
    console.log("sendClear");
    $.ajax({
        url: "http://localhost:8880/pci_bridge/clear/",
        type: "GET",
        success: handleResponseS,
        error: handleResponseE
    });
}

function sendTx() {
    console.log("sendTx");
    $.ajax({
        url: "http://localhost:8880/pci_bridge/tx/",
        type: "GET",
        success: handleResponseS,
        error: handleResponseE
    });
}

handleResponseS = function (response) {
    console.log("handleResponseSuccess():");
    var responseTextJSON;
    try {
        responseTextJSON = JSON.stringify(response);
        if(response!==null)
        {
            console.log("State: " + response.state + "  Orden: " + $("#nOrden").val() + " Orden Dev: " + $("#nOrdenD").val() + " User state: " + usrState);
            if(response.state === "READY" && ($("#nOrden").val()!=="" || $("#nOrdenD").val()!=="") && usrState !== "TALKING")
            {
                console.log("Se limpiaran campos");
                
                if($("#op").val()==="1")
                {
                    clearSale();
                    sendClear();
                }
                else
                {
                    clearDevl();
                    sendClear();
                }
                
                
                
                if($("#chkReady").is( ':checked' ))
                {
                    usrState = "READY";
                    clientLogs.log("Got to READY");
                    setREADY();
                    clientLogs.log("OP: " + $("#op").val());
                }
                
            }
            else
            {
                console.log("Operación response: " + response.op);
                if (response.op === 1) {
                    console.log("Venta");
                    if(!$("#fsSales").hasClass("show"))
                    {
                        $("#fsSales").attr("class", "show");
                        $("#fsDevs").attr("class", "hidden");
                    }
                    if(isDifSales(response))
                    {
                        fillSale(response);
                    }
                }
                if (response.op === 2) {
                    console.log("Devolucion");
                    if(!$("#fsDevs").hasClass("show"))
                    {
                        $("#fsSales").attr("class", "hidden");
                        $("#fsDevs").attr("class", "show");
                    }
                    if(isDifDev(response))
                    {
                        fillDevl(response);
                    }
                }
                if (response.op === 0 && ($("#nOrden").val()!=="" || $("#nOrdenD").val()!=="")) {
                    console.log("Sin datos.");
                    if($("#fsSales").hasClass("show"))
                    {
                        clearSale();
                    }
                    else
                    {
                        clearDevl();
                    }
                    setREADY();
                }
            }
        }
    } catch (error) {
        console.error(error);
    }

    console.log(responseTextJSON);
}

handleResponseE = function (response) {
    console.log("handleResponseError():");
    console.log("Something went wrong with the REST call.");
}

function setREADY()
{
    finesse.modules.GadgetOD.changeState(0);
}

function fillSale(obj) {
    console.log("fillSale");
    $("#nOrden").val(obj.nor);
    $("#cBanco").val(obj.bnc);
    $("#nMonto").val(obj.mnt);
    $("#bArmado").prop("checked",obj.arm);
    $("#nLocation").val(obj.loc);
    $("#cCyberS").val(obj.pcs);
    $("#IP").val(obj.ip);
    $("#op").val(obj.op);
    $("#msgVenta").text(obj.mne!==null?obj.mne:"");
	finesse.modules.GadgetOD.reSize();
}

function clearSale() {
    console.log("clearSale");
    $("#nOrden").val("");
    $("#cBanco").val("");
    $("#nMonto").val("");
    $("#bArmado").attr("checked",false);
    $("#nLocation").val("");
    $("#cCyberS").val("");
    $("#IP").val("");
    $("#msgVenta").text("");
    $("#btnVta").prop("disabled",false);
}

function fillDevl(obj){
    console.log("fillDevl");
    $("#nOrdenD").val(obj.nor);
    $("#cCard").val(obj.tct);
    $("#nMontoD").val(obj.mnt);
    $("#IP").val(obj.ip);
    $("#msgDevolucion").text(obj.mne!==null?obj.mne:"");
    $("#op").val(obj.op);
	finesse.modules.GadgetOD.reSize();
}

function clearDevl(){
    console.log("clearDevl");
    $("#nOrdenD").val("");
    $("#cCard").val("");
    $("#nMontoD").val("");
    $("#IP").val("");
    $("#msgDevolucion").text("");
    $("#btnDvl").prop("disabled",false);
}

function getVars(ext, agent)
{
    var vrs = [];
    if($("#op").val()==1)
    {
        vrs= [["callVariable1", ext],
        ["callVariable2",$("#IP").val()],
        ["callVariable3",agent],
        ["callVariable4", $("#nOrden").val()],
        ["callVariable5",$("#cBanco").val()],
        ["callVariable6",$("#nMonto").val()],
        ["callVariable7", $("#nLocation").val()],
        ["callVariable8",$("#bArmado").is( ':checked' )?1:0],
        ["callVariable9",$("#cCyberS").val()]];
    }
    else
    {
        vrs= [["callVariable1", ext],
        ["callVariable2",$("#IP").val()],
        ["callVariable3",agent],
        ["callVariable4", $("#nOrdenD").val()],
        ["callVariable5",$("#cCard").val()],
        ["callVariable6",$("#nMontoD").val()]];
    }
    console.log(vrs);
    return vrs;
}

function isValidSale()
{
    if($("#nOrden").val()=="")
    {
        return false;
    }
    if($("#cBanco").val()=="")
    {
        return false;
    }
    if($("#nMonto").val()=="")
    {
        return false;
    }
    if($("#nLocation").val()=="")
    {
        return false;
    }
    return true;
}

function isValidDev()
{
    if($("#nOrdenD").val()=="")
    {
        return false;
    }
    if($("#cCard").val()=="")
    {
        return false;
    }
    if($("#nMontoD").val()=="")
    {
        return false;
    }
    return true;
}

function isDifSales(obj)
{
    if($("#msgVenta").text()!=obj.mne)
    {
        return true;
    }
    if($("#nOrden").val()!=obj.nor)
    {
        return true;
    }
    if($("#cBanco").val()!=obj.bnc)
    {
        return true;
    }
    if($("#nMonto").val()!=obj.mnt)
    {
        return true;
    }
    if($("#bArmado").prop("checked")!=obj.arm)
    {
        return true;
    }
    if($("#nLocation").val()!=obj.loc)
    {
        return true;
    }
    if($("#cCyberS").val()!=obj.pcs)
    {
        return true;
    }
    return false;
}

function isDifDev(obj)
{
    if($("#msgDevolucion").text()!=obj.mne)
    {
        return true;
    }
    if($("#nOrdenD").val()!=obj.nor)
    {
        return true;
    }
    if($("#cCard").val()!=obj.tct)
    {
        return true;
    }
    if($("#nMontoD").val()!=obj.mnt)
    {
        return true;
    }
    return false;
}
