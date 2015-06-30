if (typeof (aspdnsf) == "undefined") {
    Type.registerNamespace('aspdnsf');
}

if (typeof (aspdnsf.Controls) == "undefined") {
    Type.registerNamespace('aspdnsf.Controls');
}

aspdnsf.Controls.RequestObserver = new Object();
aspdnsf.Controls.RequestObserver.registerButton = function(bId) {
    var btn = $get(bId);
    var req = Sys.WebForms.PageRequestManager.getInstance();
    if (btn && req) {
        var fnHandler = function(enable) {
            btn.disabled = !enable;
        }

        req.add_beginRequest(function() { fnHandler(false); });
        req.add_endRequest(function() { fnHandler(true); });
    }
}


aspdnsf.Controls.AddToCartForm = function(pId, vId) {
    this.productId = pId;
    this.variantId = vId;
    this.defaultVid = vId;
    this.validationRoutine = null;
    this.useAjaxBehavior = false;

    this.buttons = new Array();
}
aspdnsf.Controls.AddToCartForm.registerClass('aspdnsf.Controls.AddToCartForm');
aspdnsf.Controls.AddToCartForm.prototype = {

    getProductId: function () {
        return this.productId;
    },

    getVariantId: function () {
        return this.variantId;
    },

    getDefaultVariantId: function () {
        return this.defaultVid;
    },

    getElementValue: function (name) {
        var el = $get(name + '_' + this.getProductId() + '_' + this.getVariantId());
        if (el) {
            return el.value;
        }
        return '';
    },

    getQuantity: function () {
        var el = $get('Quantity_' + this.getProductId() + '_' + this.getVariantId());
        if (el) {
            return el.value;
        }
        return '';
    },

    getShippingAddressId: function () {
        return this.getElementValue('ShippingAddressID');
    },

    getCustomerEnteredPrice: function () {
        var el = $get('Price_' + this.getProductId() + '_' + this.getVariantId());
        if (el) {
            return el.value;
        }
        return '';
    },

    getColorOptions: function () {
        return this.getElementValue('Color');
    },

    getSizeOptions: function () {
        return this.getElementValue('Size');
    },

    getTextOption: function () {
        return this.getElementValue('TextOption');
    },

    getVariantStyle: function () {
        return this.getElementValue('VariantStyle');
    },

    getIsEditKit: function () {
        return this.getElementValue('IsEditKit');
    },

    getCartRecordId: function () {
        return this.getElementValue('CartRecID');
    },

    getKitItems: function () {
        return this.getElementValue('KitItems');
    },

    getUpsellItems: function () {
        var upsellItems = '';
        if (theForm.Upsell) {
            var upsellIds = new Array();
            for (var i = 0; i < theForm.Upsell.length; i++) {
                var el = theForm.Upsell[i];
                if (el.checked) {
                    upsellIds.push(el.value);
                }
            }
            upsellItems = upsellIds.toString();
        }

        return upsellItems;
    },

    getUseAjaxBehavior: function () {
        return this.useAjaxBehavior;
    },

    setUseAjaxBehavior: function (useAjax) {
        this.useAjaxBehavior = useAjax;
    },


    setValidationRoutine: function (validator) {
        this.validationRoutine = validator;
    },

    registerButton: function (bId, cartType) {
        var btn = $get(bId);
        if (btn) {
            btn.onclick = Function.createDelegate(this, function () { this.onButtonClick(cartType); });

            this.buttons.push(btn);
            aspdnsf.Controls.RequestObserver.registerButton(bId);
        }
    },

    enableDisableButtons: function (enable) {
        for (var i = 0; i < this.buttons.length; i++) {
            var btn = this.buttons[i];
            btn.disabled = !enable;
        }
    },

    onButtonClick: function (cartType) {
        var ele2 = $get('Quantity_' + this.getProductId() + '_' + this.getDefaultVariantId());
        var qty = 0;
        if (ele2) {
            qty = ele2.value;
        }

        this.variantId = document.getElementById('VariantID_' + this.getProductId() + '_' + this.getDefaultVariantId()).value;

        if (qty == 0) {
            qty = this.getQuantity();
        }

        if (cartType == 1 || (this.validationRoutine && this.validationRoutine())) {

            var useAjax = this.getUseAjaxBehavior();
            if (useAjax && cartType == 0) {
                if (typeof (aspdnsf.Controls.Minicart) != "undefined") {
                    var miniCart = aspdnsf.Controls.Minicart.getInstance();
                    if (miniCart) {
                        // make sure to suppress the autohiding of the minicart
                        // once something has been clicked outside it's panel
                        // since the last one to trigger the minicart popup display
                        // would be the addtocart button, let it stay visible
                        miniCart.set_suppressAutoHide(true);
                    }
                }

                var onCompleteCallBack = Function.createDelegate(this, this.onAddToCartComplete);

                this.enableDisableButtons(false);

                var service = new ActionService();
                if (service) {
                    service.AddToCart(this.getProductId(),
                    this.getVariantId(),
                    cartType,
                    qty,
                    this.getShippingAddressId(),
                    this.getVariantStyle(),
                    this.getTextOption(),
                    this.getCustomerEnteredPrice(),
                    this.getColorOptions(),
                    this.getSizeOptions(),
                    this.getUpsellItems(),
                    this.getCartRecordId(),
                    this.getIsEditKit(),
                    this.getKitItems(),
                    onCompleteCallBack);
                }

            }
            else {
                var args = cartType + '_' + this.getProductId() + '_' + this.getVariantId();
                __doPostBack('AddToCart', args);
                this.surpressFormPost(); // prevent unintentional second regular form posts.
            }
        }

        return false; // suppress       
    },

    surpressFormPost: function () {
        document.forms['aspnetForm'].onsubmit = function () { return false; }
    },

    onAddToCartComplete: function (status) {
        this.enableDisableButtons(true);

        if (typeof (aspdnsf.Controls.Minicart) != "undefined") {
            var miniCart = aspdnsf.Controls.Minicart.getInstance();
            if (miniCart) {

                // add an event handler for the asyncpostback end
                // so that we can display the minicart panel
                var req = Sys.WebForms.PageRequestManager.getInstance();
                var fx = function () {
                    miniCart.show();
                    // now that the panel has been already shown
                    // allow auto-hiding the panel once anything is clicked
                    // outside the content area of the minicart panel
                    miniCart.set_suppressAutoHide(false);
                    req.remove_endRequest(fx);
                }
                req.add_endRequest(fx);

                // this invocation would cause a async postback
                // in order to update and refresh the minicart display
                miniCart.refresh();
            }
        }
    }

}



