var finesse = finesse || {};
finesse.gadget = finesse.gadget || {};
finesse.container = finesse.container || {};
clientLogs = finesse.cslogger.ClientLogger || {};  // for logging

/**
 * The following comment prevents JSLint errors concerning the logFinesse function being undefined.
 * logFinesse is defined in Log.js, which should also be included by gadget the includes this file.
 */
/*global logFinesse */

/** @namespace */
finesse.modules = finesse.modules || {};
finesse.modules.GadgetOD = (function ($) {
    var user, states, dialogs, clientLogs,

    updateCallVariable = function(name, value) {
        //if (dialogObj) {
            //clientLogs.log("updateCallVariable(): Updating " + name + " to " + value + " for dialog with id: " + currentDialog.getId());

            // Call the Dialog â€” Update Call Variable Data REST API
            var config = finesse.gadget.Config;
            var url = config.scheme + "://" + config.host + ":" + config.hostPort + dialogObj.getRestUrl();
            clientLogs.log(url);
            clientLogs.log("updateCallVariable(): URL is: " + url);
            
            // Build the content body
            var contentBody = {
                                "Dialog" : {
                                    "requestedAction": finesse.restservices.Dialog.Actions.UPDATE_CALL_DATA,
                                    "mediaProperties": {
                                        "callvariables": {
                                            "CallVariable": {
                                                "name": name,
                                                "value": value
                                            }
                                        }
                                    }
                                }
                              };
            makeRequest(url, {
                method: 'PUT',
                authorization: _util.getAuthHeaderString(finesse.gadget.Config),
                contentType: 'application/xml',
                content: _util.json2xml(contentBody),
            }, {
                success: handleResponseSuccess,
                error: handleResponseError
            });
    },

    makeRequest = function (url, options, handlers) {
        var params, uuid;
        clientLogs.log("makeRequest()");
        
        // Protect against null dereferencing of options & handlers allowing its (nonexistant) keys to be read as undefined
        params = {};
        options = options || {};
        handlers.success = _util.validateHandler(handlers.success);
        handlers.error = _util.validateHandler(handlers.error);

        // Request Headers
        params[gadgets.io.RequestParameters.HEADERS] = {};

        // HTTP method is a passthrough to gadgets.io.makeRequest
        params[gadgets.io.RequestParameters.METHOD] = options.method;

        if (options.method === "GET") {
            // Disable caching for GETs
            if (url.indexOf("?") > -1) {
                url += "&";
            } else {
                url += "?";
            }
            url += "nocache=" + _util.currentTimeMillis();
        } else {
            // Generate a requestID and add it to the headers
            uuid = _util.generateUUID();
            params[gadgets.io.RequestParameters.HEADERS].requestId = uuid;
            params[gadgets.io.RequestParameters.GET_FULL_HEADERS] = "true";
        }
        
        // Add authorization to the request header if provided
        if(options.authorization) {
            params[gadgets.io.RequestParameters.HEADERS].Authorization = options.authorization;
        }

        // Add content type & body if content body is provided
        if (options.content) {
            // Content Type
            params[gadgets.io.RequestParameters.HEADERS]["Content-Type"] = options.contentType;
            // Content
            params[gadgets.io.RequestParameters.POST_DATA] = options.content;
        }

        // Call the gadgets.io.makereqest function with the encoded url
        clientLogs.log("makeRequest(): Making a REST API request to: " + url);
        gadgets.io.makeRequest(encodeURI(url), handleResponse(handlers), params);
    },

    /**
     * Handler for the response of the REST API request. This function determines if
     * the success or error handler should be called based on HTTP status code.
     *
     * @param {Object} handlers
     *     An object containing the success and error handlers.
     *
     *     {Function} handlers.success(response)
     *        A callback function to be invoked for a successful request.
     *     {Function} handlers.error(response)
     *        A callback function to be invoked for an unsuccessful request.
     */
    handleResponse = function(handlers) {
        return function (response) {
            clientLogs.log("handleResponse(): The response status code is: " + response.rc);
            
            // Send the response to the success handler if the http status
            // code is 200 or 202. Send the response to the error handler
            // otherwise.
            if ((response.rc === 200 || response.rc === 202) && handlers.success) {
                clientLogs.log("handleResponse(): Got a successful response.");
                handlers.success(response);
            } else if (handlers.error) {
                clientLogs.log("handleResponse(): Got a failure response.");
                handlers.error(response);
            } else {
                clientLogs.log("handleResponse(): Missing the success and/or error handler.");
            }
        };
    },
    
    /**
     * Handler for when the REST API response has a HTTP status code >= 200 and < 300.
     *
     * @param {Object} response
     *     An object containing the HTTP response.
     */
    handleResponseSuccess = function(response) {
        clientLogs.log("handleResponseSuccess():");
    },
    
    /**
     * Handler for when the REST API response has a HTTP status code < 200 and >= 300. 
     *
     * @param {Object} response
     *     An object containing the HTTP response.
     */
    handleResponseError = function(response) {
        clientLogs.log("handleResponseError():");
    },
    /**
     * Populates the fields in the gadget with data
     */

    render = function () {
        clientLogs.log("render");
		
        clientLogs.log("Status Agent: " + user.getState() + "Reason code: " + user.getNotReadyReasonCodeId());
        if(isXfer)
        {
            clientLogs.log("========= isXfer " + isXfer + " and state is: " + user.getState() + " ========");
            if(user.getState() === "READY" && $("#nOrden").val()!=="")
            {
                finesse.modules.GadgetOD.changeState(37);
                usrState = "WAITING";
            }
            else
            {
                isXfer = false;
                usrState = "READY";
            }
        }
        if(user.getState() != "READY")
        {
            if(user.getState() != "NOT_READY" && user.getNotReadyReasonCodeId())
            {
                send = true;
            }
            else
            {
                send = true;
            }
        }
        else
        {
            send = false;
			console.log("Se limpiaran campos");
                
            if($("#op").val()==="1")
            {
                clearSale();
            }
            else
            {
                clearDevl();
            }								   
        }
        gadgets.window.adjustHeight();
    },

    displayCall = function (dialog) {
        clientLogs.log("displayCall");
        onCall = true;
        if(dialogObj===null)
		{
			dialogObj = dialog;
		}
        clientLogs.log("dialogObj ID: " + dialogObj.getId());
        // Examples of getting data from the Dialog object (GET)
        clientLogs.log(user.getExtension());
        clientLogs.log(user.getId());
        clientLogs.log(dialog.getMediaProperties().DNIS);
        clientLogs.log(dialog.getMediaProperties().callType);
        clientLogs.log(dialog.getFromAddress());
        clientLogs.log(dialog.getToAddress());
        clientLogs.log(dialog.getState());
        
    },

    _processCall = function (dialog) {
        clientLogs.log("_processCall");
        
        displayCall(dialog);
    },

    /**
     *  Handler for additions to the Dialogs collection object. This will occur when a new
     *  Dialog is created on the Finesse server for this user.
     */
    handleNewDialog = function(dialog) {
        usrState = "TALKING";
        clientLogs.log("========= handleNewDialog " + usrState + " ========");
        clientLogs.log("New call type: " + dialog.getMediaProperties().callType);
        
        //putCID(dialog.getId());
        // call the displayCall handler
        if(dialog!==null)
        {
            clientLogs.log("---------------------------[Si hay Dialog!]-------------------------------");
            if(dialog.getMediaProperties().callType=="PREROUTE_ACD_IN" || dialog.getMediaProperties().callType=="OTHER_IN")
            {
                clientLogs.log("======================[ PREROUTE_ACD_IN || OTHER_IN ]==========================");
                clientLogs.log("======================[ ESTADO : " + user.getState() + " ]============================");
                clientLogs.log("======================[ Reason Code : " + user.getNotReadyReasonCodeId() + " ]====================");
                if(user.getState() == "NOT_READY" && user.getNotReadyReasonCodeId()==37)
                {
                    dialog.requestAction(user.getExtension(), finesse.restservices.Dialog.Actions.ANSWER, {});
                }
            }
            
        }
        else
        {
            clientLogs.log("El dialog es nulo");
        }
        displayCall(dialog);
        // add a dialog change handler in case the callvars didn't arrive yet
        dialog.addHandler('change', _processCall);
    },
     
    /**
     *  Handler for deletions from the Dialogs collection object for this user. This will occur
     *  when a Dialog is removed from this user's collection (example, end call)
     */
    handleEndDialog = function(dialog) {
        clientLogs.log("handleEndDialog()");
        // Clear the fields when the call is ended
        recoveryDialog();
        clientLogs.log("dialogObj: " + dialogObj);			  
        if(dialogObj!=null)
		{
			clientLogs.log("Type: " + dialog.getMediaProperties().callType);
			clientLogs.log("Es Transfer? " + dialog.getMediaProperties().callType.indexOf("TRANSFER"));
			if(dialog.getMediaProperties().callType.indexOf("TRANSFER")>-1)
			{
                clientLogs.log(" Comparación  ------->   dialog: " + dialog.getId() + "dialogObj: " + dialogObj.getId());
				if(dialog.getId()==dialogObj.getId())
				{
					finesse.modules.GadgetOD.changeState(37);
					if(sendTX==false)
					{
						clientLogs.log("===============This is a Client Xfer to IVR ===============");
						sendTx();
                        sendTX = true;
					}
					usrState = "WAITING";
				}
			}
			else
			{
				clientLogs.log("Type: " + dialog.getMediaProperties().callType);
				clientLogs.log("Es Transfer? " + dialog.getMediaProperties().callType.indexOf("TRANSFER"));
				if(!(dialog.getMediaProperties().callType.indexOf("TRANSFER")>-1))
				{
					clientLogs.log(" Comparación  ------->   dialog: " + dialog.getId() + "dialogObj: " + dialogObj.getId());	
					if(dialog.getId() == dialogObj.getId())
					{
                        if($("#chkReady").is( ':checked' ))
                        {
                            clientLogs.log("===========Got to READY==================");
                            setTimeout(finesse.modules.GadgetOD.changeState(0),1000);
                            clientLogs.log("OP: " + $("#op").val());
                            usrState = "READY";
                        }
						
						
						if($("#op").val()==="1")
						{
							clearSale();
						}
						else{
							clearDevl();
						}
						dialogObj = null;
                        sendTX = false;
                        isXfer = false;
						clientLogs.log("==============The call was ended===============");
					}
				}
			}
		}
        
        //
    },

    makeXfer = function(num, btn){
		
        try {
			recoveryDialog();
            clientLogs.log("Xfer dialog ID: " + dialogObj.getId());
            ///HERE:
            if(dialogObj != null)
			{
                clientLogs.log("===================== Changing State to Waiting for client ========================")
                finesse.modules.GadgetOD.changeState(37);
				clientLogs.log("===========before Update ============");
				updateVars();
				clientLogs.log("======== after Update Vars ==========");
				setTimeout(function(){
					clientLogs.log("Xfering...");
					dialogObj.initiateDirectTransfer(user.getExtension(),num,{
					success: makeCallSuccess,
					error: makeCallError
				});$("#"+btn).prop("disabled",false);},2400);
			}
        } catch (error) {
            clientLogs.log("Error(makeXfer): " + error);
        }
    },

    updateVars = function(){
        
        try {
            var vrs = getVars(user.getExtension(),user.getId());
            for(var i = 0; i<vrs.length; i++)
            {
                var name;
                var value;
                for(var j = 0; j<vrs[i].length; j++)
                {
                    if(j == 0)
                    {
                        name = vrs[i][j];
                    }
                    else if(j == 1)
                    {
                        value = vrs[i][j];
                    }
                }
                updateCallVariable(name, value);
            }
          }
          catch(error) {
            clientLogs.log("Error(updateVars)" + error);
          }
    },

    /**
     * Handler for makeCall when successful.
     */
    makeCallSuccess = function(rsp) {clientLogs.log("makeCallSuccess"); isXfer=true; sendTx();},
    
    /**
     * Handler for makeCall when error occurs.
     */
    makeCallError = function(rsp) {clientLogs.log("makeCallError"); clientLogs.log(rsp);},

    /**
     * Handler for the onLoad of a User object. This occurs when the User object is initially read
     * from the Finesse server. Any once only initialization should be done within this function.
     */
    handleUserLoad = function (userevent) {
        clientLogs.log("handleUserLoad");
        usrState = user.getState();
        clientLogs.log("State onLoad: " + usrState);
        // Get an instance of the dialogs collection and register handlers for dialog additions and
        // removals
        dialogs = user.getDialogs( {
            onCollectionAdd : handleNewDialog,
            onCollectionDelete : handleEndDialog,
            //onLoad : handleDialogsLoaded
        });
        
        render();
    },

    recoveryDialog = function ()
    {
        clientLogs.log("======== Iniciando recovery ============");
        var _dialogCollection = user.getDialogs().getCollection();
		var _dialog = null;	
			for (var dialogId in _dialogCollection) {
				
				if (_dialogCollection.hasOwnProperty(dialogId)) {
					_dialog = _dialogCollection[dialogId];
					clientLogs.log("Dialog ID: " + dialogId + " State: " + _dialog.getState() + " Type: " + _dialog.getMediaProperties().callType);
				}
			}
		dialogObj = _dialog;
        clientLogs.log("========= Dialog recovered. ============");
		
    },

    /**
     *  Handler for all User updates
     */
    handleUserChange = function(userevent) {
        clientLogs.log("handleUserChange");
		
        render();
    };

    /** @scope finesse.modules.SampleGadget */
    return {
        
        makeCall : function (number, btn) {
            if($("#op").val()==1 && isValidSale())
            {
                $("#"+btn.id).prop("disabled",true);
                try {
                    makeXfer(number, btn.id);
                }
                catch(error) {

                    clientLogs.log("Error(makeCall) " + error);
                }
            }
            else
            {
                $("#msgVenta").text("Faltan datos para procesar la venta.");
            }
            if($("#op").val()==2 && isValidDev())
            {
                $("#"+btn.id).prop("disabled",true);
                try {
                    makeXfer(number, btn.id);
                }
                catch(error) {

                    clientLogs.log("Error(makeCall) " + error);
                }
            }
            else
            {
                $("#msgDevolucion").text("Faltan datos para procesar la devolución");
            }
        },
		
		reSize : function(){
			gadgets.window.adjustHeight();
		},

        changeState : function(reasonCode){
            clientLogs.log("setUserState(): The user's current state is: " + user.getState());
            clientLogs.log("Calltype: " + dialogObj.getMediaProperties().callType);
            clientLogs.log("Reason code: " + reasonCode);
            //37 is the reasonCode for "Esperando Cliente"
            if(reasonCode == 37)
            {
                clientLogs.log("=====================Se cambiara a NOT_READY: " + states.NOT_READY+"===========");
                var rc = {id: reasonCode};
                clientLogs.log(JSON.stringify(rc));
                user.setState(states.NOT_READY, rc);
                usrState = "WAITING";
            }
            else
            {
                if (reasonCode == 0) {
                    clientLogs.log("======Se cambiara a READY========");
                    user.setState(states.READY);
                    usrState = "READY";
                }
            }
        },
        /**
         * Performs all initialization for this gadget
         */
        init : function () {
            var cfg = finesse.gadget.Config;
            _util = finesse.utilities.Utilities;

            clientLogs = finesse.cslogger.ClientLogger;  // declare clientLogs

            gadgets.window.adjustHeight();

            // Initiate the ClientServices and load the user object. ClientServices are
            // initialized with a reference to the current configuration.
            finesse.clientservices.ClientServices.init(cfg, false);

            // Initiate the clientLogs. The gadget id will be logged as a part of the message
            clientLogs.init(gadgets.Hub, "GadgetOD");

            user = new finesse.restservices.User({
                id: cfg.id,
                onLoad : handleUserLoad,
                onChange : handleUserChange
            });

            states = finesse.restservices.User.States;
            
            setInterval(sendGet, 750);
            // Initiate the ContainerServices and add a handler for when the tab is visible
            // to adjust the height of this gadget in case the tab was not visible
            // when the html was rendered (adjustHeight only works when tab is visible)
            containerServices = finesse.containerservices.ContainerServices.init();
            containerServices.addHandler(finesse.containerservices.ContainerServices.Topics.ACTIVE_TAB, function() {
                clientLogs.log("Gadget is now visible. 9");  // log to Finesse logger
                // automatically adjust the height of the gadget to show the html
                gadgets.window.adjustHeight();
            });
            containerServices.makeActiveTabReq();
        }
    };
}(jQuery));