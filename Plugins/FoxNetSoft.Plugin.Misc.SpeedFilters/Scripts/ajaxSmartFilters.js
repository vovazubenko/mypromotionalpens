
var AjaxSmartFilter = {
    loadWaiting: false,
    pagenumber: 1,
    minPriceValue: 0.00,
    maxPriceValue: 0.00,
    skipfirstloadingfilters: false,
    oldfilterstring: "",
    pagesize: null,
    defaultsortoptions: "orderby=0",
    defaultviewmode: "viewmode=grid",
    defaultpagesize: "pagesize=8",
    defaultviewzal: "viewzal=all",
    selectedviewmode: "viewmode=grid",
    lastselectedobject: null,
    separator_vendor: ",",
    separator_manufacturer: ",",
    separator_attribute1: "!==!",
    separator_attribute2: "!=>!",
    separator_attribute3: "!<>!",
    separator_specification1: ",",
    separator_specification2: "!",
    separator_specification3: ";",
    scrollafterfiltration: true,
    allowselectfiltersinoneblock: true,
    enablespecificationsfilter: false,
    enableattributesfilter: false,
    enablepricerangefilter: false,
    enablemanufacturersfilter: false,
    enablevendorsfilter: false,
    generatedUrl: "",
    init: function (minPrice, maxPrice, skipfirstloading,
        defaultvalue1, defaultvalue2, defaultvalue3,
        scrollafterfiltr, allowselectfilters, generatedUrl) {
        this.loadWaiting = false;

        this.minPriceValue = minPrice;
        this.maxPriceValue = maxPrice;
        this.skipfirstloadingfilters = skipfirstloading;

        this.defaultsortoptions = defaultvalue1;
        this.defaultviewmode = defaultvalue2;
        this.defaultpagesize = defaultvalue3;
        this.defaultviewzal = "viewzal=all";

        this.selectedviewmode = this.defaultviewmode;
        this.scrollafterfiltration = scrollafterfiltr;
        this.allowselectfiltersinoneblock = allowselectfilters;
        this.generatedUrl = generatedUrl;
    },
    enableFilters: function (enablespec, enableattrib, enableprice, enablemanufacturers, enablevendors) {
        this.enablespecificationsfilter = enablespec;
        this.enableattributesfilter = enableattrib;
        this.enablepricerangefilter = enableprice;
        this.enablemanufacturersfilter = enablemanufacturers;
        this.enablevendorsfilter = enablevendors;
    },

    setLoadWaiting: function (display) {
        displayAjaxLoading(display);
        this.loadWaiting = display;
    },

    displayAjaxLoading: function (display) {
        if (display) {
            $('.fns-speedfilters-ajax-loading-block').show();
        }
        else {
            $('.fns-speedfilters-ajax-loading-block').hide('slow');
        }
    },

    //convert AllComboBox to jDropDown
    allcomboxtojdropdown: function () {
        sf_ComboxTojDropDown("#products-pagesize");
        sf_ComboxTojDropDown("#products-orderby");
        sf_ComboxTojDropDown("#products-viewmode");
        //sf_ComboxTojDropDown("#products-viewzal");
    },

    //reload produc list
    refresh_productlist: function (controller_url, page_id, filterstring, urlparam) {
        if (this.loadWaiting != false) {
            return;
        }

        this.setLoadWaiting(true);
        $.ajax({
            cache: false,
            url: controller_url,
            data: { "page_id": page_id, "filterstring": filterstring, "urlparam": urlparam },
            type: 'POST',
            success: this.success_desktop,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },

    success_desktop: function (response) {
        if (response.success) {
            var domainUrl = location.protocol + "//" + location.host;
            var canonicalUrl = domainUrl + "/";
            if (response.updateproductselectorshtml != undefined && response.updateproductselectorshtml.length > 0) {
                var selProductSelectors = $(sf_GetProductSelectorsSelector());
                if (selProductSelectors == undefined || selProductSelectors.length == 0) {
                    $(sf_GetSelectorForPager()).before(response.updateproductselectorshtml);
                }
                else {
                    selProductSelectors.replaceWith(response.updateproductselectorshtml);
                }
                //AjaxSmartFilter.allcomboxtojdropdown();
                sf_replace_event_productselectors();
            }
            if (response.urlparam != undefined) {


                if (response.urlparam.length > 0) {
                    $('.category-page-footer-content-main').hide();
                    $("#SpeedFilterHeadrCopy").html('');
                    if ($("#footer-heading1").length > 0) {
                        $("#footer-heading1").html('');
                    }
                    if ($("#footer-heading2").length > 0) {
                        $("#footer-heading2").html('');
                    }
                    if ($("#footer-heading3").length > 0) {
                        $("#footer-heading3").html('');
                    }
                    if ($("#footer-content1").length > 0) {
                        $("#footer-content1").html('');
                    }
                    if ($("#footer-content2").length > 0) {
                        $("#footer-content2").html('');
                    }
                    if ($("#footer-content3").length > 0) {
                        $("#footer-content3").html('');
                    }

                    if (response.filterUrl != undefined && response.filterUrl != null && response.filterUrl != "") {
                        window.history.pushState(null, null, response.filterUrl);
                        if (response.canonicalUrl != "") {
                            canonicalUrl = canonicalUrl + response.canonicalUrl;
                        } else {
                            canonicalUrl = canonicalUrl + response.filterUrl;
                        }
                    }
                    else {

                        if (response.categorySlug != undefined && response.categorySlug != null && response.categorySlug != "") {
                            window.history.pushState(null, null, "");
                            var url = response.categorySlug;
                            if (response.customKeyword != "")
                                url = response.customKeyword + '_' + url;
                            window.history.pushState(null, null, url);
                            canonicalUrl = canonicalUrl + url;
                        }
                        window.location.hash = response.urlparam;
                    }
                }
                else {
                    $('.category-page-footer-content-main').show();
                    $("#SpeedFilterHeadrCopy").html('').html(response.metaModel.HeaderCopy);

                    if ($("#footer-heading1").length > 0) {
                        $("#footer-heading1").html('').html(response.metaModel.FooterTitle1);
                    }
                    if ($("#footer-heading2").length > 0) {
                        $("#footer-heading2").html('').html(response.metaModel.FooterTitle2);
                    }
                    if ($("#footer-heading3").length > 0) {
                        $("#footer-heading3").html('').html(response.metaModel.FooterTitle3);
                    }
                    if ($("#footer-content1").length > 0) {
                        $("#footer-content1").html('').html(response.metaModel.FooterContent1);
                    }
                    if ($("#footer-content2").length > 0) {
                        $("#footer-content2").html('').html(response.metaModel.FooterContent2);
                    }
                    if ($("#footer-content3").length > 0) {
                        $("#footer-content3").html('').html(response.metaModel.FooterContent3);
                    }

                   
                    if (window.location.hash.length > 1) {
                        window.location.hash = "";
                    }
                    if (response.categorySlug != undefined && response.categorySlug != null && response.categorySlug != "") {
                        window.history.pushState(null, null, "");
                        var url = response.categorySlug;
                        if (response.customKeyword != "")
                            url = response.customKeyword + '_' + url;
                        window.history.pushState(null, null, url);
                        canonicalUrl = canonicalUrl + url;
                    }
                }
            }
            if (response.updateproductsectionhtml) {
                var selPanelElement = $(sf_GetSelectorForListPanel());
                if (selPanelElement == undefined || selPanelElement.length == 0) {
                    selPanelElement = $(sf_GetSelectorForGridPanel());
                }
                if (selPanelElement == undefined || selPanelElement.length == 0) {
                    var selProductSelectors = $(sf_GetProductSelectorsSelector());
                    if (selProductSelectors == undefined || selProductSelectors.length == 0) {
                        $(".category-page .page-body").append(response.updateproductsectionhtml);
                        $(".manufacturer-page .page-body").append(response.updateproductsectionhtml);
                        $(".vendor-page .page-body").append(response.updateproductsectionhtml);
                    }
                    else {
                        selProductSelectors.after(response.updateproductsectionhtml);
                    }
                }
                else {
                    selPanelElement.replaceWith(response.updateproductsectionhtml);
                }
                AjaxSmartFilter.skipfirstloadingfilters = false;

                $.event.trigger({ type: "nopAjaxFiltersFiltrationCompleteEvent" });
                $.event.trigger({ type: "fnsSpeedFiltersAfterFiltrationEvent" });
            }
            if (response.updatepagerhtml) {
                var selPagerElement = $(sf_GetSelectorForPager());
                if (selPagerElement == undefined || selPagerElement.length == 0) {
                    $(".category-page").append(response.updatepagerhtml);
                    $(".manufacturer-page").append(response.updatepagerhtml);
                    $(".vendor-page").append(response.updatepagerhtml);
                }
                else {
                    selPagerElement.replaceWith(response.updatepagerhtml);
                }
                sf_replace_event_pager();
            }
            if (response.filterstring != undefined) {
                AjaxSmartFilter.oldfilterstring = response.filterstring;
            }
            if (AjaxSmartFilter.scrollafterfiltration) {
                $("html, body").animate({ scrollTop: 0 }, "slow");
            }
            sf_unactive_filter(response.filterableSpecification, response.filterableAttribute, response.filterableManufacturer, response.filterableVendor);

            if (response.metaModel != null) {
                //$('.page-title').find('h1').text('').text(response.metaModel.HTag);
                $('meta[name=keywords]').attr('content', response.metaModel.MetaKeyWord);
                $('meta[name=description]').attr('content', response.metaModel.MetaDescription);
                document.title = response.metaModel.MetaTitle;

                //$("#SpeedFilterHeadrCopy").remove();
                //$('.page').append('<div class="speed-filter-header-copy" id="SpeedFilterHeadrCopy"></div>');
                //$("#SpeedFilterHeadrCopy").html('').html(response.metaModel.HeaderCopy);

                //$("#SpeedFilterHTag").remove();
                //$('.page').prepend('<div class="speed-filter-htag" id="SpeedFilterHTag"></div>');
                //$("#SpeedFilterHTag").html('').html('<h1 style="font-size: 20px;text-align: center;color: #0072bc;">'+response.metaModel.HTag+'</h1>');

                $("#SpeedFilterHeadrTitle h2").html('').html(response.metaModel.HeaderTitle);
                $("#SpeedFilterHTag h1").html('').html(response.metaModel.HTag);

            }

            $("link[rel=canonical]").attr({ "href": canonicalUrl });
        }
        else {
            //error
            if (response.message) {
                displayBarNotification(response.message, 'error', 3500);
            }
        }
        $('.overlayOffCanvas').click();
        return false;
    },

    resetLoadWaiting: function () {
        AjaxSmartFilter.setLoadWaiting(false);
    },

    ajaxFailure: function () {
        $('.overlayOffCanvas').click();
        alert('Failed to load products. Please refresh the page and try one more time.');
    }
};
/*Function replace event for pager*/
function sf_replace_event_pager() {
    $(sf_GetSelectorForPager() + " li a").on("click", function () {
        var selectedvalue = $(this).attr("href");
        if (selectedvalue.length > 0) {

            var n = selectedvalue.indexOf("pagenumber=");
            if (n != -1) {
                AjaxSmartFilter.pagenumber = selectedvalue.substring(n + 11);
            }
            var n = selectedvalue.indexOf("page=");
            if (n != -1) {
                AjaxSmartFilter.pagenumber = selectedvalue.substring(n + 5);
            }
        }
        set_smart_filter();
        return false;
    });
}
/*Function onchange for product selectors*/
function sf_productselectors_onchange() {

    AjaxSmartFilter.pagenumber = 1;
    set_smart_filter()
}

/*Function replace event for product selectors*/
function sf_replace_event_productselectors() {
    $(sf_GetSelectorForProductPageSize()).attr('onchange', '').change(function (event) {
        event.preventDefault(); // cancel default behavior
        AjaxSmartFilter.pagenumber = 1;
        set_smart_filter()
        return false;
    });

    $(sf_GetSelectorForSortOptions()).attr('onchange', '').change(function (event) {
        event.preventDefault(); // cancel default behavior
        AjaxSmartFilter.pagenumber = 1;
        set_smart_filter()
        return false;
    });

    var selector = sf_GetSelectorForViewOptions();
    if ($(selector).length) {
        if ($(selector)[0].nodeName.toLowerCase() == 'select') {
            //combobox
            $(selector).attr('onchange', '').change(function (event) {
                event.preventDefault(); // cancel default behavior
                AjaxSmartFilter.pagenumber = 1;
                set_smart_filter()
                return false;
            });
        } else {
            //div
            $(selector + " a").on("click", function () {
                var selectedvalue = $(this).attr("href");
                if (selectedvalue.length > 0) {
                    var v = selectedvalue.indexOf("viewmode=");
                    if (v != -1) {
                        AjaxSmartFilter.selectedviewmode = selectedvalue.substring(v);
                        AjaxSmartFilter.pagenumber = 1;

                        $(this).parent().find("a.selected").each(function () {
                            $(this).removeClass("selected");
                        });
                        $(this).addClass("selected");
                    }
                }
                set_smart_filter();
                return false;
            });
        }
    }
    $(sf_GetViewZalOptionsDropDownSelector()).attr('onchange', '').change(function (event) {
        event.preventDefault(); // cancel default behavior
        AjaxSmartFilter.pagenumber = 1;
        set_smart_filter()
        return false;
    });
}

function sf_addparameterblock(str1, str2, separator1) {
    if (str1.length > 0)
        str1 = str1 + separator1;
    return str1 + str2;
}

/*Functions for getting value elements*/
function sf_GetProductSelectorDownValue(selectorElement, pattern) {
    /*
    //http://demo320.foxnetsoft.com/desktops?pagesize=16
    selectorElement #products-pagesize  
    pattern         pagesize
    */
    var selectedvalue = "";
    if (selectorElement != undefined && selectorElement.length > 0) {
        var selectorvalue = $(selectorElement + " option:selected").val();
        if (selectorvalue != undefined) {
            var n = selectorvalue.indexOf(pattern + "=");
            if (n != -1) {
                //http://demo320.foxnetsoft.com/desktops?pagesize=16
                //pagesize=16
                selectedvalue = selectorvalue.substring(n);
            }
        }
        //jDropDown
        if (selectedvalue.length == 0) {
            var fns_dropdown = $(selectorElement);
            if (fns_dropdown != undefined && fns_dropdown.hasClass("jDropDown")) {
                var fns_dropdowndiv = fns_dropdown.find("div p");
                if (fns_dropdowndiv != undefined) {
                    var fns_dropdownvalue = $.trim(fns_dropdowndiv.text());
                    if (fns_dropdownvalue.length > 0) {
                        fns_dropdown.find("li span").each(function () {
                            $this = $(this);
                            var fns_filter_option_name = $this.text();
                            if (fns_filter_option_name === fns_dropdownvalue) {
                                var selectorvalue = $this.parent().attr("data-dropdownoptionid");
                                var n = selectorvalue.indexOf(pattern + "=");
                                if (n != -1) {
                                    //http://demo320.foxnetsoft.com/desktops?pagesize=16
                                    //pagesize=16
                                    selectedvalue = selectorvalue.substring(n);
                                }
                            }
                        });
                    }
                }
            }
        }
    }

    //get parameters from url
    if (selectedvalue.length == 0) {
        var strblock = sf_getblock_from_url(window.location.href, "pattern");
        if (strblock.length > 0) {
            selectedvalue = pattern + "=" + strblock;
        }
    }
    return selectedvalue;
}
function sf_GetProductPageSizeDropDownValue() {
    if (AjaxSmartFilter.pagesize != undefined && AjaxSmartFilter.pagesize != null && AjaxSmartFilter.pagesize.length > 0)
        return AjaxSmartFilter.pagesize;
    return sf_GetProductSelectorDownValue(sf_GetSelectorForProductPageSize(), "pagesize");
}

function sf_GetViewOptionsDropDownValue() {

    if (AjaxSmartFilter.selectedviewmode != undefined && AjaxSmartFilter.selectedviewmode != null && AjaxSmartFilter.selectedviewmode.length > 0)
        return AjaxSmartFilter.selectedviewmode;
    var selector = sf_GetSelectorForViewOptions();
    if ($(selector).length) {
        if ($(selector)[0].nodeName.toLowerCase() == 'select') {
            return sf_GetProductSelectorDownValue(sf_GetSelectorForViewOptions(), "viewmode");
        }
    }
    return "grid";
}

function sf_GetViewZalOptionsDropDownValue() {
    return sf_GetProductSelectorDownValue(sf_GetViewZalOptionsDropDownSelector(), "viewzal");
}

function sf_GetSortOptionsDropDownValue() {
    return sf_GetProductSelectorDownValue(sf_GetSelectorForSortOptions(), "orderby");
}

function sf_GetPagerPanelValue() {
    var selector = "";
    if (sf_GetSelectorForPager() != undefined) {
        selector = "";
    }
    return selector;
}

/*Functions for getting elements*/
function sf_GetSelectorForProductPageSize() {
    var selector = $(".fns-speedfilters").attr("data-selectorproductpagesizedropdown");
    if (selector == "" || $(selector).length == 0) {
        selector = "#products-pagesize";
        $(".fns-speedfilters").attr("data-selectorproductpagesizedropdown", selector);
    }
    return selector;
}
function sf_GetSelectorForListPanel() {
    var selector = $(".fns-speedfilters").attr("data-selectorproductslistpanel");
    if (selector == "" || $(selector).length == 0) {
        selector = ".product-list";
        $(".fns-speedfilters").attr("data-selectorproductslistpanel", selector);
    }
    return selector + ":last";
}
function sf_GetSelectorForGridPanel() {
    var selector = $(".fns-speedfilters").attr("data-selectorproductsgridpanel");
    if (selector == "" || $(selector).length == 0) {
        selector = ".product-grid";
        $(".fns-speedfilters").attr("data-selectorproductsgridpanel", selector);
    }
    return selector + ":last";
}
function sf_GetSelectorForSortOptions() {
    var selector = $(".fns-speedfilters").attr("data-selectorsortoptionsdropdown");
    if (selector == "" || $(selector).length == 0) {
        selector = "#products-orderby";
        $(".fns-speedfilters").attr("data-selectorsortoptionsdropdown", selector);
    }
    return selector;
}
function sf_GetSelectorForPager() {
    var selector = $(".fns-speedfilters").attr("data-selectorpagerpanel");
    if (selector == "" || $(selector).length == 0) {
        selector = ".pager";
        $(".fns-speedfilters").attr("data-selectorpagerpanel", selector);
    }
    return selector;
}

function sf_GetProductSelectorsSelector() {
    var selector = $(".fns-speedfilters").attr("data-selectorproductselectors");
    if (selector == "" || $(selector).length == 0) {
        selector = ".product-selectors";
        $(".fns-speedfilters").attr("data-selectorproductselectors", selector);
    }
    return selector;
}

function sf_GetSelectorForViewOptions() {

    var selector = $(".fns-speedfilters").attr("data-selectorviewoptionsdropdown");
    if (selector == "" || $(selector).length == 0) {
        selector = "#products-viewmode";
        if ($(selector).length == 0) {
            selector = ".product-viewmode";
        }
        $(".fns-speedfilters").attr("data-selectorviewoptionsdropdown", selector);
    }
    return selector;
}

function sf_GetViewZalOptionsDropDownSelector() {
    var selector = "#products-viewzal";
    return selector;
}

/*Function for set filter*/
function set_smart_filter() {

    var speed_filter_urlparam = "";
    var speed_filter_filterstring = "";
    var selectedvalue = "";
    var speed_filter_is = false;
    //pagesize=12&orderby=6&viewmode=list&pagenumber=1


    //pagesize
    selectedvalue = sf_GetProductPageSizeDropDownValue();
    if (selectedvalue.length > 0) {
        if (selectedvalue != AjaxSmartFilter.defaultpagesize) {
            speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, selectedvalue, "&");
        }
        speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, selectedvalue, "&");
    }

    //orderby
    selectedvalue = sf_GetSortOptionsDropDownValue();
    if (selectedvalue.length > 0 && selectedvalue != AjaxSmartFilter.defaultsortoptions) {
        speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, selectedvalue, "&");
        speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, selectedvalue, "&");
    }
    //viewmode
    selectedvalue = sf_GetViewOptionsDropDownValue();
    if (selectedvalue.length > 0 && selectedvalue != AjaxSmartFilter.defaultviewmode) {
        speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, selectedvalue, "&");
        speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, selectedvalue, "&");
    }
    //viewzal
    selectedvalue = sf_GetViewZalOptionsDropDownValue();
    if (selectedvalue.length > 0 && selectedvalue != AjaxSmartFilter.defaultviewzal) {
        speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, selectedvalue, "&");
        speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, selectedvalue, "&");
    }
    //pagenumber

    if (AjaxSmartFilter.pagenumber > 1) {
        speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "pagenumber=" + AjaxSmartFilter.pagenumber, "&");
        speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "pagenumber=" + AjaxSmartFilter.pagenumber, "&");
    }

    //searchpage
    if ($(".basic-search") != undefined && $(".advanced-search") != undefined) {
        var fns_searchepageinputvalue = $(".search-input").find("#q").val();
        if (fns_searchepageinputvalue == undefined)
            fns_searchepageinputvalue = $(".search-input").find("#Q").val();
        if (fns_searchepageinputvalue != undefined && fns_searchepageinputvalue.trim().length > 0) {
            fns_searchepageinputvalue = fns_searchepageinputvalue.trim();
            speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "q=" + fns_searchepageinputvalue, "&");
            speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "q=" + fns_searchepageinputvalue, "&");
        }
        if ($(".search-input").find("#adv").attr("checked") == 'checked') {
            if ($(".search-input").find("#adv").attr("checked") == 'checked' ||
                $(".search-input").find("#As").attr("checked") == 'checked') {
                speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "adv=true", "&");
                speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "adv=true", "&");
            }
            if ($(".search-input").find("#isc").attr("checked") == 'checked' ||
                $(".search-input").find("#Isc").attr("checked") == 'checked') {
                speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "isc=true", "&");
                speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "isc=true", "&");
            }
            if ($(".search-input").find("#sid").attr("checked") == 'checked' ||
                $(".search-input").find("#Sid").attr("checked") == 'checked') {
                speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "sid=true", "&");
                speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "sid=true", "&");
            }
            var seacrhcategoryid = $(".search-input").find("#cid :selected").val();
            if (seacrhcategoryid == undefined) {
                seacrhcategoryid = $(".search-input").find("#Cid :selected").val();
            }
            if (seacrhcategoryid > 0) {
                speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "cid=" + seacrhcategoryid, "&");
                speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "cid=" + seacrhcategoryid, "&");
            }
            var seacrhmanufacturerid = $(".search-input").find("#mid :selected").val();
            if (seacrhmanufacturerid == undefined) {
                seacrhmanufacturerid = $(".search-input").find("#Mid :selected").val();
            }
            if (seacrhmanufacturerid > 0) {
                speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "mid=" + seacrhmanufacturerid, "&");
                speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "mid=" + seacrhmanufacturerid, "&");
            }
            var seacrhpf = $(".search-input").find("#pf").val();
            if (seacrhpf == undefined) {
                seacrhpf = $(".search-input").find("#Pf").val();
            }
            if (seacrhpf > 0) {
                speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "pf=" + seacrhpf, "&");
                speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "pf=" + seacrhpf, "&");
            }
            var seacrhpt = $(".search-input").find("#pt").val();
            if (seacrhpt == undefined) {
                seacrhpt = $(".search-input").find("#Pt").val();
            }
            if (seacrhpt > 0) {
                speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "pt=" + seacrhpt, "&");
                speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "pt=" + seacrhpt, "&");
            }
        }
    }

    //specifications
    if (AjaxSmartFilter.enablespecificationsfilter) {
        var urlparamspecall = "";
        var speed_filter_filterstring_specall = "";
        $(".specification-filter-block").each(function () {
            var urlparamspec = "";
            var speed_filter_filterstring_spec = "";
            if ($(this).find(".specification-info-block li a.filter-item-selected").length > 0) {

                if ($(this).find(".speedfltrToggleControl").hasClass('closed')) {
                    $(this).find('.speedfltrToggleControl').click();
                }
            }
            $(this).find(".specification-info-block li a.filter-item-selected").each(function () {
                var specificationid = $(this).attr('filter-option-id');
                var specificationgroupid = $(this).attr('filter-option-groupid');
                if (specificationid > 0 && specificationgroupid > 0) {
                    if (urlparamspec.length > 0)
                        urlparamspec = urlparamspec + AjaxSmartFilter.separator_specification1;
                    else
                        urlparamspec = specificationgroupid + AjaxSmartFilter.separator_specification2;

                    urlparamspec = urlparamspec + specificationid;

                    if (speed_filter_filterstring_spec.length > 0)
                        speed_filter_filterstring_spec = speed_filter_filterstring_spec + ",";
                    else
                        speed_filter_filterstring_spec = specificationgroupid + "!";
                    speed_filter_filterstring_spec = speed_filter_filterstring_spec + specificationid;
                }
            });

            //dropdown
            var fns_dropdown = $(this).find(".specification-info-block .fnsDropDown div p");
            if (fns_dropdown != undefined) {
                var fns_dropdownvalue = $.trim(fns_dropdown.text());
                if (fns_dropdownvalue.length > 0) {
                    $(this).find(".specification-info-block .fnsDropDown li").each(function () {
                        var fns_filter_option_name = $(this).attr('filter-option-name');
                        if (fns_filter_option_name === fns_dropdownvalue) {
                            var specificationid = $(this).attr('filter-option-id');
                            var specificationgroupid = $(this).attr('filter-option-groupid');
                            if (specificationid > 0 && specificationgroupid > 0) {
                                if (urlparamspec.length > 0)
                                    urlparamspec = urlparamspec + AjaxSmartFilter.separator_specification1;
                                else
                                    urlparamspec = specificationgroupid + AjaxSmartFilter.separator_specification2;
                                urlparamspec = urlparamspec + specificationid;

                                if (speed_filter_filterstring_spec.length > 0)
                                    speed_filter_filterstring_spec = speed_filter_filterstring_spec + ",";
                                else
                                    speed_filter_filterstring_spec = specificationgroupid + "!";
                                speed_filter_filterstring_spec = speed_filter_filterstring_spec + specificationid;
                            }
                        }
                    });
                }
            }

            if (urlparamspec.length > 0) {
                urlparamspecall = sf_addparameterblock(urlparamspecall, urlparamspec, AjaxSmartFilter.separator_specification3);
                $(this).find(".speedfltrclearfilter").show();
            }
            else {
                $(this).find(".speedfltrclearfilter").hide();
            }

            if (speed_filter_filterstring_spec.length > 0) {
                speed_filter_filterstring_specall = sf_addparameterblock(speed_filter_filterstring_specall, speed_filter_filterstring_spec, ";");
            }
        });
        if (urlparamspecall.length > 0) {
            speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "sFilters=" + urlparamspecall, "&");
            speed_filter_is = true;
        }
        if (speed_filter_filterstring_specall.length > 0) {
            speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "sFilters=" + speed_filter_filterstring_specall, "&");
        }
    }

    //atributes
    if (AjaxSmartFilter.enableattributesfilter) {
        var urlparamatrall = "";
        var speed_filter_filterstring_atrall = "";
        $(".attribute-filter-block").each(function () {
            var urlparamspec = "";
            var speed_filter_filterstring_spec = "";
            if ($(this).find(".attribute-info-block li a.filter-item-selected").length > 0) {
                if ($(this).find(".speedfltrToggleControl").hasClass('closed')) {
                    $(this).find('.speedfltrToggleControl').click();
                }
            }
            $(this).find(".attribute-info-block li a.filter-item-selected").each(function () {
                var atributename = $(this).attr('filter-option-name');
                var atributeid = $(this).attr('filter-option-id');
                var atributegroupid = $(this).attr('filter-option-groupid');
                if (atributename != undefined && atributename.length > 0 && atributegroupid > 0 && atributeid > 0) {
                    if (urlparamspec.length > 0)
                        urlparamspec = urlparamspec + AjaxSmartFilter.separator_attribute1;
                    else
                        urlparamspec = atributegroupid + AjaxSmartFilter.separator_attribute2;
                    urlparamspec = urlparamspec + encodeURIComponent(atributename);


                    if (speed_filter_filterstring_spec.length > 0)
                        speed_filter_filterstring_spec = speed_filter_filterstring_spec + ",";
                    else
                        speed_filter_filterstring_spec = atributegroupid + "!";
                    speed_filter_filterstring_spec = speed_filter_filterstring_spec + atributeid;
                }
            });

            //dropdown
            var fns_dropdown = $(this).find(".attribute-info-block .fnsDropDown div p");
            if (fns_dropdown != undefined) {
                var fns_dropdownvalue = $.trim(fns_dropdown.text());
                if (fns_dropdownvalue.length > 0) {
                    $(this).find(".attribute-info-block .fnsDropDown li").each(function () {
                        var fns_filter_option_name = $(this).attr('filter-option-name');
                        if (fns_filter_option_name === fns_dropdownvalue) {
                            var atributename = $(this).attr('filter-option-name');
                            var atributeid = $(this).attr('filter-option-id');
                            var atributegroupid = $(this).attr('filter-option-groupid');
                            if (atributename != undefined && atributename.length > 0 && atributegroupid > 0 && atributeid > 0) {
                                if (urlparamspec.length > 0)
                                    urlparamspec = urlparamspec + AjaxSmartFilter.separator_attribute1;
                                else
                                    urlparamspec = atributegroupid + AjaxSmartFilter.separator_attribute2;
                                urlparamspec = urlparamspec + encodeURIComponent(atributename);

                                if (speed_filter_filterstring_spec.length > 0)
                                    speed_filter_filterstring_spec = speed_filter_filterstring_spec + ",";
                                else
                                    speed_filter_filterstring_spec = atributegroupid + "!";
                                speed_filter_filterstring_spec = speed_filter_filterstring_spec + atributeid;
                            }
                        }
                    });
                }
            }

            if (urlparamspec.length > 0) {
                urlparamatrall = sf_addparameterblock(urlparamatrall, urlparamspec, AjaxSmartFilter.separator_attribute3);
                $(this).find(".speedfltrclearfilter").show();
            }
            else {
                $(this).find(".speedfltrclearfilter").hide();
            }

            if (speed_filter_filterstring_spec.length > 0) {
                speed_filter_filterstring_atrall = sf_addparameterblock(speed_filter_filterstring_atrall, speed_filter_filterstring_spec, ";");
            }

        });
        if (urlparamatrall.length > 0) {
            speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "aFilters=" + urlparamatrall, "&");
            speed_filter_is = true;
        }
        if (speed_filter_filterstring_atrall.length > 0) {
            speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "aFilters=" + speed_filter_filterstring_atrall, "&");
        }
    }

    //manufacturers
    if (AjaxSmartFilter.enablemanufacturersfilter) {
        var urlparamblock = "";
        var speed_filter_filterstring_manufacturer = "";
        if ($(".manufacturer-filter-block").find(".manufacturer-info-block li a.filter-item-selected").length > 0) {
            if ($(".manufacturer-filter-block").find('.speedfltrToggleControl').hasClass('closed')) {
                $(".manufacturer-filter-block").find('.speedfltrToggleControl').click();
            }

        }
        $(".manufacturer-info-block li a.filter-item-selected").each(function () {
            var fns_filter_option_id = $(this).attr('filter-option-id');
            if (fns_filter_option_id > 0) {
                urlparamblock = sf_addparameterblock(urlparamblock, fns_filter_option_id, AjaxSmartFilter.separator_manufacturer);
                speed_filter_filterstring_manufacturer = sf_addparameterblock(speed_filter_filterstring_manufacturer, fns_filter_option_id, ",");
            }
        });

        //dropdown
        var fns_dropdown = $(".manufacturer-info-block .fnsDropDown div p");
        if (fns_dropdown != undefined) {
            var fns_dropdownvalue = $.trim(fns_dropdown.text());
            if (fns_dropdownvalue.length > 0) {
                $(".manufacturer-info-block .fnsDropDown li").each(function () {
                    var fns_filter_option_name = $(this).attr('filter-option-name');
                    if (fns_filter_option_name === fns_dropdownvalue) {
                        var fns_filter_option_id = $(this).attr('filter-option-id');
                        if (fns_filter_option_id > 0) {
                            urlparamblock = sf_addparameterblock(urlparamblock, fns_filter_option_id, AjaxSmartFilter.separator_manufacturer);
                            speed_filter_filterstring_manufacturer = sf_addparameterblock(speed_filter_filterstring_manufacturer, fns_filter_option_id, ",");
                        }
                    }
                });
            }
        }


        if (urlparamblock.length > 0) {
            $(".manufacturer-filter-block .speedfltrclearfilter").show();
            speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "mFilters=" + urlparamblock, "&");
            speed_filter_is = true;
        }
        else {
            $(".manufacturer-filter-block .speedfltrclearfilter").hide();
        }

        if (speed_filter_filterstring_manufacturer.length > 0) {
            speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "mFilters=" + speed_filter_filterstring_manufacturer, "&");
        }
    }

    //vendors
    if (AjaxSmartFilter.enablevendorsfilter) {
        var urlparamblock = "";
        var speed_filter_filterstring_vendor = "";
        if ($(".vendor-filter-block").find(".vendor-info-block li a.filter-item-selected").length > 0) {
            if ($(".vendor-filter-block").find('.speedfltrToggleControl').hasClass('closed')) {
                $(".vendor-filter-block").find('.speedfltrToggleControl').click();
            }

        }
        $(".vendor-info-block li a.filter-item-selected").each(function () {
            var fns_filter_option_id = $(this).attr('filter-option-id');
            //alert("vendorid="+vendorid);
            if (fns_filter_option_id > 0) {
                urlparamblock = sf_addparameterblock(urlparamblock, fns_filter_option_id, AjaxSmartFilter.separator_vendor);
                speed_filter_filterstring_vendor = sf_addparameterblock(speed_filter_filterstring_vendor, fns_filter_option_id, ",");
            }
        });


        //dropdown
        var fns_dropdown = $(".vendor-info-block .fnsDropDown div p");
        if (fns_dropdown != undefined) {
            var fns_dropdownvalue = $.trim(fns_dropdown.text());
            if (fns_dropdownvalue.length > 0) {
                $(".vendor-info-block .fnsDropDown li").each(function () {
                    var fns_filter_option_name = $(this).attr('filter-option-name');
                    if (fns_filter_option_name === fns_dropdownvalue) {
                        var fns_filter_option_id = $(this).attr('filter-option-id');
                        if (fns_filter_option_id > 0) {
                            urlparamblock = sf_addparameterblock(urlparamblock, fns_filter_option_id, AjaxSmartFilter.separator_vendor);
                            speed_filter_filterstring_vendor = sf_addparameterblock(speed_filter_filterstring_vendor, fns_filter_option_id, ",");
                        }
                    }
                });
            }
        }

        if (urlparamblock.length > 0) {
            $(".vendor-filter-block .speedfltrclearfilter").show();
            speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "vFilters=" + urlparamblock, "&");
            speed_filter_is = true;
        }
        else {
            $(".vendor-filter-block .speedfltrclearfilter").hide();
        }

        if (speed_filter_filterstring_vendor.length > 0) {
            speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "vFilters=" + speed_filter_filterstring_vendor, "&");
        }
    }

    //pricerange
    if (AjaxSmartFilter.enablepricerangefilter) {
        var valprice0 = $("#priceslider-range").slider("values", 0);
        var valprice1 = $("#priceslider-range").slider("values", 1);
        if (AjaxSmartFilter.minPriceValue != valprice0 || AjaxSmartFilter.maxPriceValue != valprice1) {
            $("#clearpricerangefilter").show();
            //show_allclose=true;
            var urlparamprice = "";
            var speed_filter_filterstring_price = "";
            if (AjaxSmartFilter.minPriceValue != valprice0) {
                urlparamprice = urlparamprice + "From-" + valprice0;
                speed_filter_filterstring_price = speed_filter_filterstring_price + valprice0;
            }

            if (AjaxSmartFilter.maxPriceValue != valprice1) {
                urlparamprice = sf_addparameterblock(urlparamprice, "To-" + valprice1, "!-#!");
                speed_filter_filterstring_price = speed_filter_filterstring_price + "-" + valprice1;
            }
            speed_filter_urlparam = sf_addparameterblock(speed_filter_urlparam, "prFilter=" + urlparamprice, "&");
            speed_filter_filterstring = sf_addparameterblock(speed_filter_filterstring, "price=" + speed_filter_filterstring_price, "&");
            speed_filter_is = true;
        }
        else
            $("#clearpricerangefilter").hide();
    }

    if (speed_filter_urlparam.length > 0)
        speed_filter_urlparam = "#/" + speed_filter_urlparam;

    if (speed_filter_is)
        $(".clear-filter-options-all").show();
    else
        $(".clear-filter-options-all").hide();


    if (AjaxSmartFilter.oldfilterstring != speed_filter_filterstring || AjaxSmartFilter.skipfirstloadingfilters) {
        var speed_filter_controller = $(".fns-speedfilters").attr("data-speedfiltercontoller");
        var speed_filter_datapageid = $(".fns-speedfilters").attr("data-datapageid");
        if (speed_filter_controller != undefined && speed_filter_controller.length > 0 && speed_filter_datapageid != undefined && speed_filter_datapageid.length > 0) {
            AjaxSmartFilter.refresh_productlist(speed_filter_controller, speed_filter_datapageid, speed_filter_filterstring, speed_filter_urlparam);
        }
    }
}

function sf_jdropdown_callback(index, val) {
    AjaxSmartFilter.pagenumber = 1;
    set_smart_filter();
}

function sf_disablechecbox(checboxobject) {
    if (AjaxSmartFilter.allowselectfiltersinoneblock && AjaxSmartFilter.lastselectedobject != null
        && AjaxSmartFilter.lastselectedobject.is($(checboxobject).parent().parent())) {
        return true;
    }
    if ($(checboxobject).hasClass("filter-item-selected")) {
        $(checboxobject).addClass("filter-item-disabled-selected");
    }
    if ($(checboxobject).hasClass("filter-item-unselected")) {
        $(checboxobject).addClass("filter-item-disabled-unselected");
    }
    return false;
}
function sf_enablechecbox(checboxobject) {
    //set all active
    if ($(checboxobject).hasClass("filter-item-disabled-unselected")) {
        $(checboxobject).removeClass("filter-item-disabled-unselected");
    }
    if ($(checboxobject).hasClass("filter-item-disabled-selected")) {
        $(checboxobject).removeClass("filter-item-disabled-selected");
    }
}
function sf_unactive_filter(filterableSpecification, filterableAttribute, filterableManufacturer, filterableVendor) {
    //specifications
    if (AjaxSmartFilter.enablespecificationsfilter) {
        $(".specification-filter-block li a[filter-option-id]").each(function () {
            //set all active
            sf_enablechecbox(this);
            //set inactive
            var fns_filter_option_id = parseInt($(this).attr('filter-option-id'));
            if (fns_filter_option_id > 0 && filterableSpecification != undefined && Object.prototype.toString.call(filterableSpecification) === '[object Array]' && filterableSpecification.length > 0) {
                if (jQuery.inArray(fns_filter_option_id, filterableSpecification) == -1) {
                    sf_disablechecbox(this);
                }
            }
        });
        //dropdown
        $(".specification-info-block .fnsDropDown li[filter-option-id]").each(function () {
            $(this).show();
            var fns_filter_option_id = parseInt($(this).attr('filter-option-id'));
            if (fns_filter_option_id > 0 && filterableSpecification != undefined && Object.prototype.toString.call(filterableSpecification) === '[object Array]' && filterableSpecification.length > 0) {
                if (jQuery.inArray(fns_filter_option_id, filterableSpecification) == -1) {
                    if (AjaxSmartFilter.allowselectfiltersinoneblock && AjaxSmartFilter.lastselectedobject != null
                        && AjaxSmartFilter.lastselectedobject.is($(this).parent().parent())) {
                        return true;
                    }
                    $(this).hide();
                }
            }
        });
    }

    //atributes
    if (AjaxSmartFilter.enableattributesfilter) {
        $(".attribute-filter-block li a[filter-option-id]").each(function () {
            //set all active
            sf_enablechecbox(this);
            //set inactive
            var fns_filter_option_id = parseInt($(this).attr('filter-option-id'));
            if (fns_filter_option_id > 0 && filterableAttribute != undefined && Object.prototype.toString.call(filterableAttribute) === '[object Array]' && filterableAttribute.length > 0) {
                if (jQuery.inArray(fns_filter_option_id, filterableAttribute) == -1) {
                    sf_disablechecbox(this);
                }
            }
        });
        //dropdown
        $(".attribute-info-block .fnsDropDown li[filter-option-id]").each(function () {
            $(this).show();
            var fns_filter_option_id = parseInt($(this).attr('filter-option-id'));
            if (fns_filter_option_id > 0 && filterableAttribute != undefined && Object.prototype.toString.call(filterableAttribute) === '[object Array]' && filterableAttribute.length > 0) {
                if (jQuery.inArray(fns_filter_option_id, filterableAttribute) == -1) {
                    if (AjaxSmartFilter.allowselectfiltersinoneblock && AjaxSmartFilter.lastselectedobject != null
                        && AjaxSmartFilter.lastselectedobject.is($(this).parent().parent())) {
                        return true;
                    }
                    $(this).hide();
                }
            }
        });
    }

    //manufacturers
    if (AjaxSmartFilter.enablemanufacturersfilter) {
        $(".manufacturer-info-block li a[filter-option-id]").each(function () {
            //set all active
            sf_enablechecbox(this);
            //set inactive
            var fns_filter_option_id = parseInt($(this).attr('filter-option-id'));
            if (fns_filter_option_id > 0 && filterableManufacturer != undefined && Object.prototype.toString.call(filterableManufacturer) === '[object Array]' && filterableManufacturer.length > 0) {
                if (jQuery.inArray(fns_filter_option_id, filterableManufacturer) == -1) {
                    sf_disablechecbox(this);
                }
            }
        });

    }
    //vendors
    if (AjaxSmartFilter.enablevendorsfilter) {
        $(".venfor-info-block li a[filter-option-id]").each(function () {
            //set all active
            sf_enablechecbox(this);
            //set inactive
            var fns_filter_option_id = parseInt($(this).attr('filter-option-id'));
            if (fns_filter_option_id > 0 && filterableVendor != undefined && Object.prototype.toString.call(filterableVendor) === '[object Array]' && filterableVendor.length > 0) {
                if (jQuery.inArray(fns_filter_option_id, filterableVendor) == -1) {
                    sf_disablechecbox(this);
                }
            }
        });
    }
}

//initialize filers url
function sf_initialize_filers() {
    var strblock = "";
    var filterUrl = AjaxSmartFilter.generatedUrl;
    var url = decodeURI(window.location.href);
    if (filterUrl != '')
        url = filterUrl;

    //url = filterUrl;
    //url = "http://demo320.foxnetsoft.com/desktops#/prFilter=From-543!-#!To-1296&manFilters=1!##!2!##!45!##!84!##!745&venFilters=1!##!2!##!45!##!84!##!745&";

    //demo320.foxnetsoft.com/desktops#/pagesize=2&orderby=15&viewmode=list&prFilter=From-618
    //pricerange
    if (AjaxSmartFilter.enablepricerangefilter) {
        //&prFilter=From-1245!-#!To-2145&
        strblock = sf_getblock_from_url(url, "prFilter");
        if (strblock.length > 0) {
            var price1 = null;
            var price2 = null;
            //From-1245!-#!To-2145&
            var blockarray = strblock.split("!-#!");
            for (i = 0; i < blockarray.length; i++) {
                if (blockarray[i].indexOf("From-") != -1) {
                    price1 = parseInt(blockarray[i].replace('From-', ''));
                }
                if (blockarray[i].indexOf("To-") != -1) {
                    price2 = parseInt(blockarray[i].replace('To-', ''));
                }
            }
            if (price1 < 0) { price1 = null; }
            if (price2 < 0) { price2 = null; }
            if (price1 > price2 && price2 > 0 && price1 > 0) { price1 = null; price2 = null; }
            if (price1 < AjaxSmartFilter.minPriceValue) { price1 = null; }
            if (price2 > AjaxSmartFilter.maxPriceValue) { price2 = null; }

            if (price1 > 0 || price2 > 0) {
                var pricerangeslider = $("#priceslider-range");
                if (pricerangeslider != undefined) {
                    if (price1 > 0) {
                        pricerangeslider.slider("values", 0, price1);
                        $("#fns-selected-pricemin").text(price1);
                    }
                    if (price2 > 0) {
                        pricerangeslider.slider("values", 1, price2);
                        $("#fns-selected-pricemax").text(price2);
                    }
                }
            }
        }
    }

    //specification
    if (AjaxSmartFilter.enablespecificationsfilter) {
        //specFilters=2!#-!5!##!6!-#!3m!#-!7
        //sFilters=2!5,6;3!7
        strblock = sf_getblock_from_url(url, "sFilters");
        if (strblock.length == 0)
            strblock = sf_getblock_from_url(url, "specFilters");
        if (strblock.length > 0) {
            var blockarraygroup = strblock.split(AjaxSmartFilter.separator_specification3);
            if (strblock.indexOf("!-#!") >= 0)
                blockarraygroup = strblock.split("!-#!");
            for (i = 0; i < blockarraygroup.length; i++) {
                //2!#-!5!##!6   3!#-!7
                //2!5,6   3!7
                var lengthmarker = AjaxSmartFilter.separator_specification2.length;
                posblock = blockarraygroup[i].indexOf(AjaxSmartFilter.separator_specification2);
                if (blockarraygroup[i].indexOf("!#-!") >= 0) {
                    posblock = blockarraygroup[i].indexOf("!#-!");
                    lengthmarker = AjaxSmartFilter.separator_specification2.length;
                }
                if (posblock != -1) {
                    var groupid = parseInt(blockarraygroup[i].substring(0, posblock));
                    strblock = blockarraygroup[i].substring(posblock + lengthmarker);
                    //5!##!6      7
                    //5,6      7
                    var blockarray = strblock.split(AjaxSmartFilter.separator_specification1);
                    if (strblock.indexOf("!##!") >= 0)
                        blockarray = strblock.split("!##!");
                    for (j = 0; j < blockarray.length; j++) {
                        var id = parseInt(blockarray[j]);
                        if (id > 0) {
                            $(".specification-filter-block li a[filter-option-id='" + id + "']").removeClass("filter-item-unselected").addClass("filter-item-selected");

                            //dropdown
                            $(".specification-info-block .fnsDropDown").each(function () {
                                var fns_filter_option_name = $(this).find("li[filter-option-id='" + id + "']").attr('filter-option-name');
                                if (fns_filter_option_name != undefined && fns_filter_option_name.length > 0) {
                                    $(this).find("div p:first").text(fns_filter_option_name);
                                }
                            });
                        }
                    }
                }
            }
        }
    }

    //atributes
    if (AjaxSmartFilter.enableattributesfilter) {
        //aFilters=3!=>!400 GB!<>!6!=>!2 GB
        //aFilters=3!=>!320 GB!==!400 GB!<>!4!=>!Vista Home
        strblock = sf_getblock_from_url(url, "aFilters");
        if (strblock.length == 0)
            strblock = sf_getblock_from_url(url, "attrFilters");
        if (strblock.length > 0) {
            var blockarraygroup = strblock.split(AjaxSmartFilter.separator_attribute3);
            if (strblock.indexOf("!-#!") >= 0)
                blockarraygroup = strblock.split("!-#!");
            for (i = 0; i < blockarraygroup.length; i++) {
                //3!=>!320 GB!==!400 GB!
                //4!=>!Vista Home
                var lengthmarker = AjaxSmartFilter.separator_attribute2.length;
                posblock = blockarraygroup[i].indexOf(AjaxSmartFilter.separator_attribute2);
                if (blockarraygroup[i].indexOf("!#-!") >= 0) {
                    posblock = blockarraygroup[i].indexOf("!#-!");
                    lengthmarker = AjaxSmartFilter.separator_attribute2.length;
                }
                if (posblock != -1) {
                    var groupid = parseInt(blockarraygroup[i].substring(0, posblock));
                    strblock = blockarraygroup[i].substring(posblock + lengthmarker);
                    //320 GB!==!400 GB
                    //Vista Home
                    var blockarray = strblock.split(AjaxSmartFilter.separator_attribute1);
                    if (strblock.indexOf("!##!") >= 0)
                        blockarray = strblock.split("!##!");
                    for (j = 0; j < blockarray.length; j++) {
                        if (blockarray[j].length > 0) {
                            $(".attribute-filter-block li a[filter-option-name='" + blockarray[j] + "'][filter-option-groupid='" + groupid + "']").each(function () {
                                /*var atributegroupid=$(this).attr('filter-option-groupid');
                                if (atributegroupid>0 && atributegroupid==groupid)
                                {*/
                                $(this).removeClass("filter-item-unselected").addClass("filter-item-selected");
                                /*}*/
                            });

                            //dropdown
                            $(".attribute-info-block .fnsDropDown").each(function () {
                                var fns_filter_option_name = $(this).find("li[filter-option-name='" + blockarray[j] + "'][filter-option-groupid='" + groupid + "']").attr('filter-option-name');
                                if (fns_filter_option_name != undefined && fns_filter_option_name.length > 0) {
                                    $(this).find("div p:first").text(fns_filter_option_name);
                                }
                            });

                        }
                    }
                }
            }
        }
    }

    //manufacturer
    if (AjaxSmartFilter.enablemanufacturersfilter) {
        //manFilters=1!##!2!##!45!##!84!##!745&
        //mFilters=1,2,45,84,745&
        strblock = sf_getblock_from_url(url, "mFilters");
        if (strblock.length == 0)
            strblock = sf_getblock_from_url(url, "manFilters");
        if (strblock.length > 0) {
            var blockarray = strblock.split(AjaxSmartFilter.separator_manufacturer);
            if (strblock.indexOf("!##!") >= 0)
                blockarray = strblock.split("!##!");
            for (var x in blockarray) {
                var id = parseInt(blockarray[x]);
                if (id > 0) {
                    $(".manufacturer-info-block li a[filter-option-id='" + id + "']").removeClass("filter-item-unselected").addClass("filter-item-selected");

                    //dropdown
                    $(".manufacturer-info-block .fnsDropDown").each(function () {
                        var fns_filter_option_name = $(this).find("li[filter-option-id='" + id + "']").attr('filter-option-name');
                        if (fns_filter_option_name != undefined && fns_filter_option_name.length > 0) {
                            $(this).find("div p:first").text(fns_filter_option_name);
                        }
                    });
                }
            }
        }
    }

    //vendor
    if (AjaxSmartFilter.enablevendorsfilter) {
        //venFilters=1!##!2!##!45!##!84!##!745&
        //vFilters=1,2,45,84,745&
        strblock = sf_getblock_from_url(url, "vFilters");
        if (strblock.length == 0)
            strblock = sf_getblock_from_url(url, "venFilters");
        if (strblock.length > 0) {
            var blockarray = strblock.split(AjaxSmartFilter.separator_vendor);
            if (strblock.indexOf("!##!") >= 0)
                blockarray = strblock.split("!##!");
            for (var x in blockarray) {
                var id = parseInt(blockarray[x]);
                if (id > 0) {
                    $(".vendor-info-block li a[filter-option-id='" + id + "']").removeClass("filter-item-unselected").addClass("filter-item-selected");

                    //dropdown
                    $(".vendor-info-block .fnsDropDown").each(function () {
                        var fns_filter_option_name = $(this).find("li[filter-option-id='" + id + "']").attr('filter-option-name');
                        if (fns_filter_option_name != undefined && fns_filter_option_name.length > 0) {
                            $(this).find("div p:first").text(fns_filter_option_name);
                        }
                    });
                }
            }
        }
    }
    // pagesize=12&orderby=6&viewmode=list&pagenumber=1

    //pagesize
    //pagesize=12&
    strblock = sf_getblock_from_url(url, "pagesize");
    if (strblock.length > 0) {
        var id = parseInt(strblock);
        if (id > 0) {
            var selectedvalue = "";
            var selectorElement = sf_GetSelectorForProductPageSize();
            if (selectorElement != undefined && selectorElement.length > 0) {
                var has_select_value = false;
                var selectorvalue = $(selectorElement + " option").each(function () {
                    this.selected = false;
                    var optionvalue = $(this).attr("value");
                    if (optionvalue.indexOf("pagesize=" + id) != -1) {
                        $(this).attr("selected", "selected");
                        has_select_value = true;
                    }
                    /*else
                    {
                        $(this).attr("selected", "");
                    }*/
                });
                if (!has_select_value) {
                    $(selectorElement + " :first").attr("selected", "selected");
                }
            }

            //jdropdown
            $(".product-page-size .jDropDown li span").each(function () {
                var fns_filter_option_name = $(this).parent().attr("data-dropdownoptionid");
                if (fns_filter_option_name.indexOf("pagesize=" + id) != -1) {
                    $(".product-page-size .jDropDown").find("div p:first").text($(this).text());
                }
            });
        }
    }

    //orderby
    //orderby=6&
    strblock = sf_getblock_from_url(url, "orderby");
    if (strblock.length > 0) {
        var id = parseInt(strblock);
        if (id > 0) {
            var selectedvalue = "";
            var selectorElement = sf_GetSelectorForSortOptions();
            if (selectorElement != undefined && selectorElement.length > 0) {
                var has_select_value = false;
                var selectorvalue = $(selectorElement + " option").each(function () {
                    this.selected = false;
                    var optionvalue = $(this).attr("value");
                    if (optionvalue.indexOf("orderby=" + id) != -1) {
                        $(this).attr("selected", "selected");
                        has_select_value = true;
                    }
                    /*else
                    {
                        $(this).attr("selected", "");
                    }*/
                });
                if (!has_select_value) {
                    $(selectorElement + " :first").attr("selected", "selected");
                }
            }
            //jdropdown
            $(".product-sorting .jDropDown li span").each(function () {
                var fns_filter_option_name = $(this).parent().attr("data-dropdownoptionid");
                if (fns_filter_option_name.indexOf("orderby=" + id) != -1) {
                    $(".product-sorting .jDropDown").find("div p:first").text($(this).text());
                }
            });
        }
    }

    //viewmode
    //viewmode=grid& viewmode=list&
    strblock = sf_getblock_from_url(url, "viewmode");
    if (strblock.length > 0) {
        var selectedvalue = "";
        var selectorElement = sf_GetSelectorForViewOptions();
        if (selectorElement != undefined && selectorElement.length > 0) {
            if ($(selectorElement)[0].nodeName.toLowerCase() == 'select') {
                //combobox
                var has_select_value = false;
                var selectorvalue = $(selectorElement + " option").each(function () {
                    this.selected = false;
                    var optionvalue = $(this).attr("value");
                    if (optionvalue.indexOf("viewmode=" + strblock) != -1) {
                        $(this).attr("selected", "selected");
                        has_select_value = true;
                    }
                });
                if (!has_select_value) {
                    $(selectorElement + " :first").attr("selected", "selected");
                }
            }
            else {
                //div
                var selectorvalue = $(selectorElement + " a").each(function () {
                    if ($(this).hasClass(strblock)) {
                        if (!$(this).hasClass("selected")) {
                            $(this).addClass("selected");
                        }
                        AjaxSmartFilter.selectedviewmode = "viewmode=" + strblock;
                    }
                    else {
                        $(this).removeClass("selected");
                    }
                });
            }
        }
        //jdropdown
        $(".product-viewmode .jDropDown li span").each(function () {
            var fns_filter_option_name = $(this).parent().attr("data-dropdownoptionid");
            if (fns_filter_option_name.indexOf("viewmode=" + strblock) != -1) {
                $(".product-viewmode .jDropDown").find("div p:first").text($(this).text());
            }
        });
    }

    //viewzal
    //viewzal=zal
    strblock = sf_getblock_from_url(url, "viewzal");
    if (strblock.length > 0) {
        var selectedvalue = "";
        var selectorElement = sf_GetViewZalOptionsDropDownSelector();
        if (selectorElement != undefined && selectorElement.length > 0) {
            var has_select_value = false;
            var selectorvalue = $(selectorElement + " option").each(function () {
                this.selected = false;
                var optionvalue = $(this).attr("value");
                if (optionvalue.indexOf("viewzal=" + strblock) != -1) {
                    $(this).attr("selected", "selected");
                    has_select_value = true;
                }
                /*else
                {
                    $(this).attr("selected", "");
                }*/
            });
            if (!has_select_value) {
                $(selectorElement + " :first").attr("selected", "selected");
            }
        }
        //jdropdown
        $(".product-viewzal .jDropDown li span").each(function () {
            var fns_filter_option_name = $(this).parent().attr("data-dropdownoptionid");
            if (fns_filter_option_name.indexOf("viewzal=" + strblock) != -1) {
                $(".product-viewzal .jDropDown").find("div p:first").text($(this).text());
            }
        });
    }

    AjaxSmartFilter.pagenumber = 1
    //pagenumber
    //pagenumber=6&

    strblock = sf_getblock_from_url(url.toLowerCase(), "pagenumber");
    if (strblock.length > 0) {
        var id = parseInt(strblock);
        if (id > 0) {
            AjaxSmartFilter.pagenumber = id;
        }
    }
    //veryfy url for filters block

    if (filterUrl.length > 0 || window.location.hash.length > 2 || AjaxSmartFilter.skipfirstloadingfilters) {
        set_smart_filter();
    }
}

//get parameters block from url
function sf_getblock_from_url(url, nameblock) {
    var posblock = -1
    var strblock = "";
    posblock = url.indexOf(nameblock + "=");
    if (posblock != -1) {
        var strblock = url.substring(posblock + nameblock.length + 1);
        posblock = strblock.indexOf("&");
        if (posblock != -1) {
            strblock = strblock.substring(0, posblock);
        }
    }
    return strblock;
}

//convert ComboBox to jDropDown
function sf_ComboxTojDropDown(comboxSelector) {
    if (comboxSelector == undefined || comboxSelector.length == 0)
        return
    var comboxElement = $(comboxSelector);
    if (comboxElement == undefined || comboxElement.length != 1)
        return
    var comboxElementSelectedValue = 0;
    var comboxElementIteration = 0;
    var comboxselectedtext = comboxElement.find("option:selected").text();
    var comboxjDropDown = $("<div>").attr({
        id: comboxElement.attr("id"),
        'class': "fnsDropDown jDropDown"
    });
    comboxjDropDown.append("<div><p>" + comboxselectedtext + "</p></div>");
    comboxElement.find("option").map(function () {
        var $this = $(this);
        if ($this.attr("selected") == "selected")
            comboxElementSelectedValue = comboxElementIteration;
        comboxElementIteration = comboxElementIteration + 1;
        return $("<li>").attr("data-dropdownoptionid", $this.attr("value")).append("<span>" + $this.text() + "</span>").get();
    }).appendTo($("<ul>")).parent().appendTo(comboxjDropDown);
    comboxjDropDown.replaceAll(comboxElement);
    comboxjDropDown.jDropDown({ selected: comboxElementSelectedValue, callback: sf_productselectors_onchange });
}

$(document).ready(function () {
    $('body').addClass('filterbody');
});

