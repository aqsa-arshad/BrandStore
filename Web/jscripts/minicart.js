if (typeof (aspdnsf) == "undefined") {
    Type.registerNamespace('aspdnsf');
}

if (typeof (aspdnsf.Controls) == "undefined") {
    Type.registerNamespace('aspdnsf.Controls');
}

aspdnsf.Controls.MinicartControl = function(element) {
    aspdnsf.Controls.MinicartControl.initializeBase(this, [element]);

    this.refreshDelegate = null;
    this.extender = null;
    this.popupFocused = false;
    this.suppressAutoHide = false;    

    this.document_onclick_delegate = Function.createDelegate(this, this.document_onclick);

    this.popup_onfocus_delegate = Function.createDelegate(this, this.popup_onfocus);
    this.popup_onblur_delegate = Function.createDelegate(this, this.popup_onblur);
}
aspdnsf.Controls.MinicartControl.prototype = {

    initialize: function() {
        aspdnsf.Controls.MinicartControl.callBaseMethod(this, 'initialize');

        var e = this.get_element();
        $addHandler(e, 'mouseover', this.popup_onfocus_delegate);
        $addHandler(e, 'mouseout', this.popup_onblur_delegate);

        $addHandler(document, 'click', this.document_onclick_delegate);
    },

    dispose: function() {
        var e = this.get_element();
        $removeHandler(e, 'mouseover', this.popup_onfocus_delegate);
        $removeHandler(e, 'mouseout', this.popup_onblur_delegate);
        
        $removeHandler(document, 'click', this.document_onclick_delegate);

        aspdnsf.Controls.MinicartControl.callBaseMethod(this, 'dispose');
    },

    setRefreshDelegate: function(dlgRefresh) {
        this.refreshDelegate = dlgRefresh;
    },

    refresh: function() {
        if (this.refreshDelegate) {
            this.refreshDelegate();
        }
    },

    set_suppressAutoHide: function(supress) {
        this.suppressAutoHide = supress;
    },

    setExtender: function(extId) {
        // we'll only use the extender id as reference
        // since we can get another instance of the behavior object
        // everytime an async postback is made, so we therefore
        // find the most current extender by id to get the correct reference
        this.extender = extId;
    },

    show: function() {
        var ext = $find(this.extender);
        if (ext && ext.get_Collapsed() == true) {
            // add a catch error block since we're toggling
            // the call to the collapsePanel programmatically
            // and which requires an "event" parameter, which can only be supplied
            // by the browser itself that raises the event during HTML dom interaction
            try {
                ext.expandPanel(Sys.UI.DomEvent);
            }
            catch (e) { }
        }
    },

    tryHide: function() {
        // don't hide the minicart if this flag is turned on
        // usually the one will turn this on is a control or event
        // outside the bounds of this minicart panel
        // and would like to keep the panel visible i.e. async addtocart button
        if (this.suppressAutoHide) return;

        // first check if the minicart popup is currently displayed
        // or we have focus on this control 
        if (!this.popupFocused) {
            var ext = $find(this.extender);
            if (ext) {
                if (ext.get_Collapsed() == true) return;
                // add a catch error block since we're toggling
                // the call to the collapsePanel programmatically
                // and which requires an "event" parameter, which can only be supplied
                // by the browser itself that raises the event during HTML dom interaction
                try {
                    ext.collapsePanel(Sys.UI.DomEvent);
                }
                catch (e) { }
            }
        }
    },

    popup_onfocus: function() {
        // set the flag that the current focused area is the minicart panel
        this.popupFocused = true;
    },

    popup_onblur: function() {
        // minicart panel just lost focus
        this.popupFocused = false;
    },

    document_onclick: function() {
        this.tryHide();
    }
}
aspdnsf.Controls.MinicartControl.registerClass('aspdnsf.Controls.MinicartControl', Sys.UI.Control);

aspdnsf.Controls.Minicart = new Object();
aspdnsf.Controls.Minicart.setInstance = function(instance) {
    this.instance = instance;
}
aspdnsf.Controls.Minicart.getInstance = function() {
    return this.instance;
}


