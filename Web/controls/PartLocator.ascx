<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PartLocator.ascx.cs" Inherits="controls_PartLocator" %>
    
    <script src="App_Themes/Skin_3/PartsReplacement/bower_components/modernizr/modernizr.js"></script>  
    <script src="App_Themes/Skin_3/PartsReplacement/scripts/c0536796.vendor.js"></script>
    <script src="App_Themes/Skin_3/PartsReplacement/scripts/b578e01e.main.js"></script>
    <script src="App_Themes/Skin_3/PartsReplacement/scripts/d41d8cd9.plugins.js"></script>
        <div id="ng-app" ng-app="jwPartsModule">
           
            <div data-breakpoint="" class="container2" id="jwPartsModule" ng-controller="slideController">
                <div class="panels">
                    <div id="windowOrDoor" class="jwPanel">
                        <div class="jwTab" ng-click="clickTab($event)"><span class="tabTitle">Window or door?</span><div class="badge">1</div>
                        </div>
                        <div class="wrapper clearfix">
                            <header>
                                <h2>1. Window or door?</h2>
                            </header>
                            <div class="row">
                                <div class="col-sm-6">
                                    <div class="jwThumbnail" ng-click="openPanel('styleOfWindow')">
                                        <div class="preview thumbLarge">
                                            <div data-animation-thumbnail="" data-src="App_Themes/Skin_3/PartsReplacement/images/1bb94b80.double_hung_window.png"></div>
                                        </div>
                                        <div class="caption">Vinyl Window</div>
                                    </div>
                                </div>
                                <div class="col-sm-6">
                                    <div class="jwThumbnail">
                                        <div class="chooser" data-identifier="identifierA" data-image="App_Themes/Skin_3/PartsReplacement/images/359bd01d.fpoTargetImage.jpg" data-targets="[
                                                 {
                                                    &quot;posX&quot;:30,
                                                    &quot;posY&quot;:80,
                                                    &quot;width&quot;:30,
                                                    &quot;height&quot;:10,
                                                    &quot;identifier&quot;:&quot;targetA&quot;
                                                 },
                                                 {
                                                    &quot;posX&quot;:65,
                                                    &quot;posY&quot;:5,
                                                    &quot;width&quot;:10,
                                                    &quot;height&quot;:44,
                                                    &quot;identifier&quot;:&quot;targetB&quot;
                                                 }
                                             ]">
                                        </div>
                                        <div class="caption">Sliding Door</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="styleOfWindow" class="jwPanel">
                        <div class="jwTab" ng-click="clickTab($event)"><span class="tabTitle">Style of Window?</span><div class="badge">2</div>
                        </div>
                        <div class="wrapper clearfix">
                            <header>
                                <h2>2. Style of window?</h2>
                                <a class="btn hidden-xs" ng-click="startOver()" href="#">Start Over</a></header>
                            <div class="row">
                                <div class="col-sm-4">
                                    <div ng-click="openPanel('hingeSide')" class="jwThumbnail">
                                        <div class="preview">
                                            <img src="http://placehold.it/600x600" alt=""></div>
                                        <div class="caption">Double Hung</div>
                                    </div>
                                </div>
                                <div class="col-sm-4">
                                    <div class="jwThumbnail">
                                        <div class="preview">
                                            <img src="http://placehold.it/600x600" alt=""></div>
                                        <div class="caption">Single Hung</div>
                                    </div>
                                </div>
                                <div class="col-sm-4">
                                    <div class="jwThumbnail">
                                        <div class="preview">
                                            <img src="http://placehold.it/600x600" alt=""></div>
                                        <div class="caption">Awning</div>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-sm-4">
                                    <div class="jwThumbnail">
                                        <div class="preview">
                                            <img src="http://placehold.it/600x600" alt=""></div>
                                        <div class="caption">Sliding</div>
                                    </div>
                                </div>
                                <div class="col-sm-4">
                                    <div class="jwThumbnail">
                                        <div class="preview">
                                            <img src="http://placehold.it/600x600" alt=""></div>
                                        <div class="caption">Casement</div>
                                    </div>
                                </div>
                                <div class="col-sm-4"><a class="btn visible-xs" ng-click="startOver()" href="#">Start Over</a></div>
                            </div>
                        </div>
                    </div>
                    <div id="hingeSide" class="jwPanel">
                        <div class="jwTab" ng-click="clickTab($event)"><span class="tabTitle">Hinge side?</span><div class="badge">3</div>
                        </div>
                        <div class="wrapper clearfix">
                            <header>
                                <h2>3. Hinge side?</h2>
                                <a class="btn hidden-xs" ng-click="startOver()" href="#">Start Over</a></header>
                            <div class="row">
                                <div class="col-sm-6">
                                    <div class="jwThumbnail" ng-click="openPanel('selectPartHinge')">
                                        <div class="preview thumbLarge">
                                            <img src="http://placehold.it/600x600" alt=""></div>
                                        <div class="caption">Left side</div>
                                    </div>
                                </div>
                                <div class="col-sm-6">
                                    <div class="jwThumbnail" ng-click="openPanel('selectPartHinge')">
                                        <div class="preview thumbLarge">
                                            <img src="http://placehold.it/600x600" alt=""></div>
                                        <div class="caption">Right side</div>
                                    </div>
                                </div>
                                <div class="col-sm-6"><a class="btn visible-xs" ng-click="startOver()" href="#">Start Over</a></div>
                            </div>
                        </div>
                    </div>
                    <div id="selectPartHinge" class="jwPanel">
                        <div class="jwTab" ng-click="clickTab($event)"><span class="tabTitle">Select your part.</span><div class="badge">4</div>
                        </div>
                        <div class="wrapper clearfix">
                            <header>
                                <h2>4. Select your part.</h2>
                                <a class="btn hidden-xs" ng-click="startOver()" href="#">Start Over</a></header>
                            <div class="row">
                                <div class="col-sm-6">
                                    <div class="jwThumbnail">
                                        <div class="chooser" data-identifier="identifierA" data-image="App_Themes/Skin_3/PartsReplacement/images/359bd01d.fpoTargetImage.jpg" data-targets="[
                                                 {
                                                    &quot;posX&quot;:35,
                                                    &quot;posY&quot;:84,
                                                    &quot;width&quot;:25,
                                                    &quot;height&quot;:10,
                                                    &quot;identifier&quot;:&quot;targetA&quot;
                                                 },
                                                 {
                                                    &quot;posX&quot;:71,
                                                    &quot;posY&quot;:63,
                                                    &quot;width&quot;:8,
                                                    &quot;height&quot;:16,
                                                    &quot;identifier&quot;:&quot;targetB&quot;
                                                 }
                                             ]">
                                        </div>
                                        <div class="caption">Casement Window - Left Hinge</div>
                                    </div>
                                </div>
                                <div class="col-sm-6">Instructional copy FPO<br>
                                    Select the part on the window that you are looking to replace<div class="bottomContent">Can't find what you're looking for?<div><a href="#" class="btn">Contact JELD-WEN</a></div>
                                    </div>
                                </div>
                                <div class="col-sm-6"><a class="btn visible-xs" ng-click="startOver()" href="#">Start Over</a></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
 
