Type.registerNamespace('aspdnsf.Controls.Extenders');

aspdnsf.Controls.Extenders.TextStateChangePostbackBehavior = function(element) {
    aspdnsf.Controls.Extenders.TextStateChangePostbackBehavior.initializeBase(this, [element]);

    this._timeout = 500;
    this._timer = null;
    this._text = '';
    this._postbackOnChangeRoutine = null;
    this._debugMode = false;
    this._monitorTextChanged = false;
}
aspdnsf.Controls.Extenders.TextStateChangePostbackBehavior.prototype = {

    initialize: function() {
        aspdnsf.Controls.Extenders.TextStateChangePostbackBehavior.callBaseMethod(this, 'initialize');

        this._text = this.trim(this.get_element().value);

        // own the postback routine handler
        this._postbackOnChangeRoutine = this.get_element().onchange;
        this.get_element().onchange = null;

        $addHandlers(this.get_element(), { keydown: this._onKeyDown }, this);
    },

    dispose: function() {
        if (this._timer) {
            this._timer = null;
        }

        $clearHandlers(this.get_element());

        aspdnsf.Controls.Extenders.TextStateChangePostbackBehavior.callBaseMethod(this, 'dispose');

    },

    _onKeyDown: function() {
        this._stopTimer();
        this._startTimer();
    },

    trim: function(text) {
        return text.replace(/^\s+|\s+$/g, "");
    },

    _onTimerTick: function() {
        this._stopTimer();

        if (this.get_element()) {
            var elementText = this.trim(this.get_element().value);
            
            
            var currentlyEmpty = elementText == '';
            if (!currentlyEmpty && this._monitorTextChanged) {
                if (elementText != this._text) {
                    this._doPostbackOnStateChangeRoutine();
                }
            }
            else {
                var previouslyEmpty = this._text == '';

                if ((previouslyEmpty && !currentlyEmpty) ||
                    (currentlyEmpty && !previouslyEmpty)) {
                    this._text = elementText;

                    this._doPostbackOnStateChangeRoutine();
                }
            }
        }
    },

    _doPostbackOnStateChangeRoutine: function() {
        if (this._debugMode) {
            Sys.Debug.trace(this.get_element().id + ': Text State Changed');
            return;
        }

        if (this._postbackOnChangeRoutine) {
            this._postbackOnChangeRoutine();
        }
    },

    _startTimer: function() {
        this._timer = window.setTimeout(Function.createDelegate(this, this._onTimerTick), this._timeout);
    },

    _stopTimer: function() {
        if (this._timer != null) {
            window.clearTimeout(this._timer);
        }
        this._timer = null;
    },

    get_timeout: function() {
        return this._timeout;
    },

    set_timeout: function(value) {
        this._timeout = value;
        this.raisePropertyChanged('timeout');
    },

    get_debugMode: function() {
        return this._debugMode;
    },

    set_debugMode: function(value) {
        this._debugMode = value;
    },

    get_monitorTextChanged: function() {
        return this._monitorTextChanged;
    },

    set_monitorTextChanged: function(value) {
        this._monitorTextChanged = value;
    }

}

aspdnsf.Controls.Extenders.TextStateChangePostbackBehavior.registerClass('aspdnsf.Controls.Extenders.TextStateChangePostbackBehavior', AjaxControlToolkit.BehaviorBase);

