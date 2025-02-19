﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace NakedObjects.Selenium {
    /// <summary>
    /// Tests for the detailed operation of dialogs, including parameter rendering,
    /// choices, auto-complete, default values, formatting, and validation
    /// </summary>
    public abstract class DialogTestsRoot : AWTest {
        public virtual void PasswordParam() {
            Debug.WriteLine(nameof(PasswordParam));
            GeminiUrl("object?i1=View&o1=___1.Person--11656&as1=open&d1=ChangePassword&f1_oldPassword=%22%22&f1_newPassword=%22%22&f1_confirm=%22%22");
            //Check that params marked with DataType.Password show up as input type="password" for browser to obscure
            wait.Until(dr => dr.FindElements(By.CssSelector("input")).Count(el => el.GetAttribute("type") == "password") == 3);
        }

        public virtual void ScalarChoicesParm() {
            Debug.WriteLine(nameof(ScalarChoicesParm));
            Url(OrdersMenuUrl);
            OpenActionDialog("Orders By Value");
            SelectDropDownOnField("#ordering1", "Descending");
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Orders By Value");
            AssertTopItemInListIs("SO51131");
        }

        public virtual void TestCancelDialog() {
            Debug.WriteLine(nameof(TestCancelDialog));
            Url(OrdersMenuUrl);
            OpenActionDialog("Orders By Value");
            CancelDialog();
            WaitUntilElementDoesNotExist(".dialog");
        }

        public virtual void FieldsRetainedWhenNavigatingAwayAndBack() {
            Debug.WriteLine(nameof(FieldsRetainedWhenNavigatingAwayAndBack));
            GeminiUrl("home?m1=CustomerRepository&d1=FindIndividualCustomerByName");
            ClearFieldThenType("#firstname1", "arthur");
            ClearFieldThenType("#lastname1", "brent");
            //1. Navigating away and back retains values
            ClickRecentButton();
            WaitForView(Pane.Single, PaneType.Recent);
            ClickBackButton();
            WaitForView(Pane.Single, PaneType.Home);
            wait.Until(d => d.FindElement(By.CssSelector("#firstname1")).GetAttribute("value") == "arthur");
            wait.Until(d => d.FindElement(By.CssSelector("#lastname1")).GetAttribute("value") == "brent");
        }

        public virtual void ReopeningADialogThatWasntCancelledDoesNotRetainFields() {
            Debug.WriteLine(nameof(ReopeningADialogThatWasntCancelledDoesNotRetainFields));
            ClickRecentButton();
            WaitForView(Pane.Single, PaneType.Recent);
            Click(HomeIcon());
            WaitForView(Pane.Single, PaneType.Home);
            GoToMenuFromHomePage("Customers");
            OpenSubMenu("Individuals");
            OpenActionDialog("Find Individual Customer By Name");
            wait.Until(d => d.FindElement(By.CssSelector("#firstname1")).GetAttribute("value") == "");
            wait.Until(d => d.FindElement(By.CssSelector("#lastname1")).GetAttribute("value") == "");
        }

        public virtual void ScalarParmShowsDefaultValue() {
            Debug.WriteLine(nameof(ScalarParmShowsDefaultValue));
            Url(CustomersMenuUrl);
            GetObjectActions(CustomerServiceActions);
            OpenActionDialog("Find Customer By Account Number");
            var input = WaitForCss(".value input");
            Assert.AreEqual("AW", input.GetAttribute("value"));
        }

        public virtual void DateTimeParmKeepsValue() {
            Debug.WriteLine(nameof(DateTimeParmKeepsValue));
            GeminiUrl("object?o1=___1.Customer--29923&as1=open");
            OpenSubMenu("Orders");
            OpenActionDialog("Search For Orders");
            var fromDate = WaitForCss("#fromdate1");
            Assert.AreEqual("1 Jan 2000", fromDate.GetAttribute("value")); //Default field value
            ClearFieldThenType("#fromdate1", "01/09/2007");
            ClearFieldThenType("#todate1", "01/04/2008");
            Thread.Sleep(1000);
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Search For Orders");
            var details = WaitForCss(".summary .details");
            Assert.AreEqual("Page 1 of 1; viewing 2 of 2 items", details.Text);
        }

        public virtual void TimeSpanParm() {
            Debug.WriteLine(nameof(TimeSpanParm));
            GeminiUrl("object?i1=View&o1=___1.Shift--1&as1=open");
            OpenActionDialog("Change Times");

            var rand = new Random();

            var end = new TimeSpan(rand.Next(23), rand.Next(59), 0);
            ClearFieldThenType("nof-dialog input#endtime1", end.ToString("hhmm"));

            var start = new TimeSpan(rand.Next(23), rand.Next(59), 0);
            ClearFieldThenType("nof-dialog input#starttime1", start.ToString("hhmm"));
            Thread.Sleep(2000);

            Click(OKButton());
            WaitForTextEquals(".property", 2, "Start Time:\r\n" + start.ToString(@"hh\:mm"));
            WaitForTextEquals(".property", 3, "End Time:\r\n" + end.ToString(@"hh\:mm"));
        }

        public virtual void RefChoicesParmKeepsValue() {
            Debug.WriteLine(nameof(RefChoicesParmKeepsValue));
            Url(ProductServiceUrl);
            OpenActionDialog("List Products By Sub Category");
            Thread.Sleep(500);
            SelectDropDownOnField("#subcategory1", "Forks");
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "List Products By Sub Category");
            wait.Until(dr => dr.FindElements(By.CssSelector("td.reference"))[0].Text == "HL Fork");
        }

        public virtual void MultipleRefChoicesDefaults() {
            Debug.WriteLine(nameof(MultipleRefChoicesDefaults));
            Url(ProductServiceUrl);
            OpenActionDialog("List Products By Sub Categories");

            wait.Until(dr => new SelectElement(WaitForCss("select#subcategories1")).AllSelectedOptions.ElementAt(0).Text == "Mountain Bikes");
            wait.Until(dr => new SelectElement(WaitForCss("select#subcategories1")).AllSelectedOptions.ElementAt(1).Text == "Touring Bikes");

            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "List Products By Sub Categories");
            AssertTopItemInListIs("Mountain-100 Black, 38");
        }

        public virtual void MultipleRefChoicesChangeDefaults() {
            Debug.WriteLine(nameof(MultipleRefChoicesChangeDefaults));
            GeminiUrl("home");
            WaitForView(Pane.Single, PaneType.Home);
            Url(ProductServiceUrl);
            OpenActionDialog("List Products By Sub Categories");
            wait.Until(dr => new SelectElement(WaitForCss("select#subcategories1")).AllSelectedOptions.Count == 2);
            wait.Until(dr => new SelectElement(WaitForCss("select#subcategories1")).AllSelectedOptions.ElementAt(0).Text == "Mountain Bikes");
            wait.Until(dr => new SelectElement(WaitForCss("select#subcategories1")).AllSelectedOptions.ElementAt(1).Text == "Touring Bikes");

            br.FindElement(By.CssSelector(".value  select option[label='Mountain Bikes']")).Click();
            wait.Until(dr => new SelectElement(WaitForCss("select#subcategories1")).AllSelectedOptions.Count == 1);

            IKeyboard kb = ((IHasInputDevices) br).Keyboard;
            kb.PressKey(Keys.Control);
            br.FindElement(By.CssSelector(".value  select option[label='Road Bikes']")).Click();
            kb.ReleaseKey(Keys.Control);
            wait.Until(dr => new SelectElement(WaitForCss("select#subcategories1")).AllSelectedOptions.Count == 2);
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "List Products By Sub Categories");
            wait.Until(dr => dr.FindElement(By.CssSelector(".summary .details")).Text == "Page 1 of 4; viewing 20 of 65 items");
            AssertTopItemInListIs("Road-150 Red, 44");
        }

        public virtual void ChoicesDefaults() {
            Debug.WriteLine(nameof(ChoicesDefaults));
            Url(ProductServiceUrl);
            OpenActionDialog("Find By Product Line And Class");

            var slctPl = new SelectElement(WaitForCss("select#productline1"));
            var slctPc = new SelectElement(WaitForCss("select#productclass1"));

            Assert.AreEqual("M", slctPl.SelectedOption.Text);
            Assert.AreEqual("H", slctPc.SelectedOption.Text);

            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Find By Product Line And Class");
            AssertTopItemInListIs("HL Mountain Frame - Silver, 42");
        }

        public virtual void ChoicesOptional() {
            Debug.WriteLine(nameof(ChoicesOptional));
            //Test that a field with choices that is optional can be left blank
            GeminiUrl("home?m1=ProductRepository&d1=FindByOptionalProductLinesAndClasses&f1_productLine=%5B%5D&f1_productClass=%5B%5D");
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Find By Optional Product Lines And Classes");
        }

        public virtual void ChoicesChangeDefaults() {
            Debug.WriteLine(nameof(ChoicesChangeDefaults));
            Url(ProductServiceUrl);
            OpenActionDialog("Find By Product Line And Class");

            SelectDropDownOnField("#productline1", "R");
            SelectDropDownOnField("#productclass1", "L");

            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Find By Product Line And Class");
            AssertTopItemInListIs("LL Road Frame - Black, 58");
        }

        public virtual void ConditionalChoices() {
            Debug.WriteLine(nameof(ConditionalChoices));
            GeminiUrl("home?m1=ProductRepository");
            WaitForView(Pane.Single, PaneType.Home);
            OpenActionDialog("List Products");
            SelectDropDownOnField("#category1", "Clothing");
            WaitForCss("#subcategory1");

            wait.Until(d => new SelectElement(WaitForCss("#subcategory1")).Options.ElementAt(0).Text == "*");

            wait.Until(d => new SelectElement(WaitForCss("#subcategory1")).Options.ElementAt(1).Text == "Bib-Shorts");

            SelectDropDownOnField("#category1", "Accessories");
            wait.Until(d => new SelectElement(WaitForCss("#subcategory1")).Options.ElementAt(1).Text == "Bike Racks");

            var msg = OKButton().AssertIsDisabled().GetAttribute("title");
            Assert.AreEqual("Missing mandatory fields: Sub Category; ", msg);

            SelectDropDownOnField("#subcategory1", "Bike Racks");
            msg = OKButton().AssertIsEnabled().GetAttribute("title");
            Assert.AreEqual("", msg);
        }

        public virtual void ConditionalChoicesDefaults() {
            Debug.WriteLine(nameof(ConditionalChoicesDefaults));
            Url(ProductServiceUrl);
            OpenActionDialog("Find Products By Category");
            var slctCs = new SelectElement(WaitForCss("select#categories1"));

            Assert.AreEqual("Bikes", slctCs.SelectedOption.Text);

            Thread.Sleep(1000);

            var slct = new SelectElement(WaitForCss("select#subcategories1"));

            Assert.AreEqual(2, slct.AllSelectedOptions.Count);
            Assert.AreEqual("Mountain Bikes", slct.AllSelectedOptions.First().Text);
            Assert.AreEqual("Road Bikes", slct.AllSelectedOptions.Last().Text);

            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Find Products By Category");
            AssertTopItemInListIs("Mountain-100 Black, 38");
        }

        public virtual void ConditionalChoicesMultiple() {
            Debug.WriteLine(nameof(ConditionalChoicesMultiple));
            Url(ProductServiceUrl);

            OpenActionDialog("Find Products By Category");

            var slctCs = new SelectElement(WaitForCss("select#categories1"));

            Assert.AreEqual("Bikes", slctCs.SelectedOption.Text);

            wait.Until(d => new SelectElement(WaitForCss("select#subcategories1")).AllSelectedOptions.Count == 2);

            var slct = new SelectElement(WaitForCss("select#subcategories1"));

            Assert.AreEqual("Mountain Bikes", slct.AllSelectedOptions.First().Text);
            Assert.AreEqual("Road Bikes", slct.AllSelectedOptions.Last().Text);

            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Find Products By Category");
            AssertTopItemInListIs("Mountain-100 Black, 38");
        }

        public virtual void ParameterDescriptionRenderedAsPlaceholder() {
            Debug.WriteLine(nameof(ParameterDescriptionRenderedAsPlaceholder));
            GeminiUrl("home?m1=CustomerRepository&d1=FindStoreByName");
            var name = WaitForCss("input#name1");
            Assert.AreEqual("* partial match", name.GetAttribute("placeholder"));
        }

        public virtual void BooleanParams() {
            Debug.WriteLine(nameof(BooleanParams));
            GeminiUrl("home?m1=EmployeeRepository&d1=ListEmployees2");
            //None of the fields should have a mandatory indicator
            var prams = WaitForCss(".parameter .value", 4);
            Assert.AreEqual("", prams[0].Text);
            Assert.AreEqual("", prams[1].Text);
            Assert.AreEqual("", prams[2].Text);
            Assert.AreEqual("", prams[3].Text);

            var current = WaitForCss("#current1");
            var married = WaitForCss("#married1");
            var salaried = WaitForCss("#salaried1");
            var older = WaitForCss("#olderthan501");

            Assert.AreEqual(null, current.GetAttribute("checked"));
            Assert.AreEqual(null, married.GetAttribute("checked"));
            Assert.AreEqual(null, salaried.GetAttribute("checked"));
            Assert.AreEqual("true", older.GetAttribute("checked"));

            OKButton().AssertIsEnabled();
        }

        public virtual void NullableBooleanParams() {
            Debug.WriteLine(nameof(NullableBooleanParams));
            GeminiUrl("home?m1=EmployeeRepository&d1=ListEmployees");

            //Test for visibility of the * mandatory indicator. Should only be on the first one
            //because the other mandatory param has a default value and therefore can't be selected
            //back to null anyway.
            var prams = WaitForCss(".parameter .value", 4);
            Assert.AreEqual("*", prams[0].Text);
            Assert.AreEqual("", prams[1].Text);
            Assert.AreEqual("", prams[2].Text);
            Assert.AreEqual("", prams[3].Text);

            var current = WaitForCss("#current1");
            var married = WaitForCss("#married1");
            var salaried = WaitForCss("#salaried1");
            var older = WaitForCss("#olderthan501");

            //The following is a VERY limited form of testing, because the tri-state checkbox does
            //not show up as Html -  it is managed by the JavaScript
            Assert.IsNull(current.GetAttribute("checked"));
            Assert.IsNull(married.GetAttribute("checked"));
            Assert.IsNull(salaried.GetAttribute("checked")); //This should really be 'false' but doesn't show up to Html that way
            Assert.AreEqual("true", older.GetAttribute("checked"));

            //Check that last one is tri-state  -  three clicks to get back to checked
            Click(older);
            Assert.IsNull(older.GetAttribute("checked"));
            Click(older);
            Assert.IsNull(older.GetAttribute("checked"));
            Click(older);
            Assert.AreEqual("true", older.GetAttribute("checked"));

            //But the top one is bi-state because it is mandatory
            Click(current);
            Assert.AreEqual("true", current.GetAttribute("checked"));
            Click(current);
            Assert.IsNull(current.GetAttribute("checked"));
            Click(current);
            Assert.AreEqual("true", current.GetAttribute("checked"));
        }

        public virtual void WarningShownWithinDialogAndInFooter() {
            Debug.WriteLine(nameof(WarningShownWithinDialogAndInFooter));
            GeminiUrl("home?m1=CustomerRepository&d1=FindCustomerByAccountNumber&f1_accountNumber=%22AW%22");
            ClearFieldThenType("#accountnumber1", "AW1");
            Click(OKButton());
            wait.Until(dr => dr.FindElement(By.CssSelector(".co-validation")).Text.Contains("No matching object found"));
            wait.Until(dr => dr.FindElement(By.CssSelector(".footer .warnings")).Text.Contains("No matching object found"));
        }

        public virtual void DefaultReferenceParamRendersCorrectly() {
            Debug.WriteLine(nameof(DefaultReferenceParamRendersCorrectly));
            //To test a previous bug, where reference was beign rendered as a Url, not its title
            GeminiUrl("object?i1=View&o1=___1.Person--18542&as1=open");
            OpenActionDialog("Create Letter");
            wait.Until(dr => dr.FindElement(By.CssSelector(".droppable")).GetAttribute("value").StartsWith("Zeiter Weg 9922"));
        }

        public virtual void QueryOnlyActionDialogPersists() {
            Debug.WriteLine(nameof(QueryOnlyActionDialogPersists));
            //To test:
            //Query only action OK, left click & go back, right click & remains open
            GeminiUrl("home?m1=ProductRepository");
            WaitForView(Pane.Single, PaneType.Home);
            OpenActionDialog("Find Product By Name");
            ClearFieldThenType("#searchstring1", "a");
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Find Product By Name");
            ClickBackButton();
            WaitForView(Pane.Single, PaneType.Home);
            var field = WaitForCss("#searchstring1");
            Assert.AreEqual("a", field.GetAttribute("value"));
            ClearFieldThenType("#searchstring1", "b");
            RightClick(OKButton());
            WaitForView(Pane.Right, PaneType.List, "Find Product By Name");
            WaitForView(Pane.Left, PaneType.Home);
            field = WaitForCss("#searchstring1");
            Assert.AreEqual("b", field.GetAttribute("value"));
        }

        public virtual void PotentActionDialogDisappearsAndFieldsNotRemembered() {
            Debug.WriteLine(nameof(PotentActionDialogDisappearsAndFieldsNotRemembered));
            GeminiUrl("object?i1=View&o1=___1.SalesOrderHeader--57732&as1=open");
            OpenActionDialog("Add Multi Line Comment");
            ClearFieldThenType("#comment1", "foo");
            Click(OKButton());
            WaitUntilElementDoesNotExist(".dialog");
            // robustness hack
            Thread.Sleep(1000);
            OpenActionDialog("Add Multi Line Comment");
            Assert.AreEqual("", WaitForCss("#comment1").GetAttribute("value"));
        }

        //Test for #49
        public virtual void NoResultFoundMessageLeavesDialogOpen() {
            Debug.WriteLine(nameof(NoResultFoundMessageLeavesDialogOpen));
            GeminiUrl("home?m1=CustomerRepository&d1=FindCustomerByAccountNumber");
            ClearFieldThenType("#accountnumber1", "AW66666");
            Click(OKButton());
            Thread.Sleep(1000);
            WaitForTextEquals(".co-validation", "No matching object found");
            ClearFieldThenType("#accountnumber1", "AW00000194");
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.Object, "Mechanical Brake Manufacturers, AW00000194");
        }

        #region Auto Complete

        public virtual void AutoCompleteParm() {
            Debug.WriteLine(nameof(AutoCompleteParm));
            Url(SalesServiceUrl);
            WaitForView(Pane.Single, PaneType.Home, "Home");
            OpenActionDialog("List Accounts For Sales Person");
            wait.Until(dr => dr.FindElement(By.CssSelector("#sp1")).GetAttribute("placeholder") == "* (auto-complete or drop)");
            ClearFieldThenType("#sp1", "Valdez");

            // nof custom autocomplete
            wait.Until(d => d.FindElements(By.CssSelector(".suggestions a")).Count > 0);
            // angular/material autocomplete
            //wait.Until(d => d.FindElements(By.CssSelector("md-option")).Count > 0);
            //As the match has not yet been selected,the field is invalid, so...
            WaitForTextEquals(".validation", "Pending auto-complete...");
            OKButton().AssertIsDisabled().AssertHasTooltip("Invalid fields: Sp; ");

            // nof custom autocomplete
            Click(WaitForCss(".suggestions a"));
            // angular/material autocomplete
            //Click(WaitForCss("md-option"));
            WaitForCss("#sp1.link-color6");
            OKButton().AssertIsEnabled();
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "List Accounts For Sales Person");
        }

        public virtual void AutoCompleteParmDefault() {
            Debug.WriteLine(nameof(AutoCompleteParmDefault));
            Url(ProductServiceUrl);
            WaitForView(Pane.Single, PaneType.Home, "Home");
            OpenActionDialog("Find Product");
            Assert.AreEqual("Adjustable Race", WaitForCss(".value input[type='text']").GetAttribute("value"));
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.Object, "Adjustable Race");
        }

        public virtual void ClearingAutoCompleteTextClearsUnderlyingReference() {
            Debug.WriteLine(nameof(ClearingAutoCompleteTextClearsUnderlyingReference));
            Url(ProductServiceUrl);
            OpenActionDialog("Find Product");
            Assert.AreEqual("Adjustable Race", WaitForCss(".value input[type='text']").GetAttribute("value"));
            ClearFieldThenType("#product1", "");
            OKButton().AssertIsDisabled().AssertHasTooltip("Missing mandatory fields: Product; ");
            ClearFieldThenType("#product1", "xxx");
            //As the match has not yet been selected,the field is invalid, so...
            WaitForTextEquals(".validation", "Pending auto-complete...");
            OKButton().AssertIsDisabled().AssertHasTooltip("Invalid fields: Product; ");
        }

        public virtual void AutoCompleteParmShowSingleItem() {
            Debug.WriteLine(nameof(AutoCompleteParmShowSingleItem));
            Url(ProductServiceUrl);
            OpenActionDialog("Find Product");
            // for some reason "BB" doesn't work in test - works OK manually - "BB Ball" seems to work
            ClearFieldThenType("#product1", "BB Ball");

            // nof custom 
            wait.Until(dr => dr.FindElement(By.CssSelector("ul li a")).Text == "BB Ball Bearing");
            var item = br.FindElement(By.CssSelector("ul li a"));

            // angular/material
            //wait.Until(dr => dr.FindElement(By.CssSelector("md-option")).Text == "BB Ball Bearing");
            //var item = br.FindElement(By.CssSelector("md-option"));
            //As the match has not yet been selected,the field is invalid, so...
            WaitForTextEquals(".validation", "Pending auto-complete...");
            Click(item);
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.Object, "BB Ball Bearing");
        }

        public virtual void AutoCompleteScalarField() {
            Debug.WriteLine(nameof(AutoCompleteScalarField));
            GeminiUrl("object?i1=View&o1=___1.SalesOrderHeader--54461&as1=open&d1=AddComment&f1_comment=%22%22");
            WaitForView(Pane.Single, PaneType.Object, "SO54461");

            // "parc" doesn't work in test (ok manually) "parcel" works
            ClearFieldThenType("#comment1", "parcel");

            // nof custom
            wait.Until(d => d.FindElements(By.CssSelector("ul li a")).Count == 2);
            // angular/material
            //wait.Until(d => d.FindElements(By.CssSelector("md-option")).Count == 2);
        }

        public virtual void AutoCompleteOptionalParamNotSelected() {
            Debug.WriteLine(nameof(AutoCompleteOptionalParamNotSelected));
            //Test written against a bug in 8.0.0-beta9
            GeminiUrl("home?m1=OrderRepository&d1=FindOrders");
            OKButton().AssertIsEnabled();
            ClearFieldThenType("#customer1", "AW00000");
            //but don't select the item
            //TODO: Message should change to Invalid fields
            OKButton().AssertIsDisabled().AssertHasTooltip("Invalid fields: Customer; ");
            ClearFieldThenType("#customer1", "AW00000456");

            // nof custom
            wait.Until(dr => dr.FindElement(By.CssSelector("ul li a")).Text == "Riding Excursions, AW00000456");
            var item = br.FindElement(By.CssSelector("ul li a"));
            // angular/material
            //wait.Until(dr => dr.FindElement(By.CssSelector("md-option")).Text == "Riding Excursions, AW00000456");
            //var item = br.FindElement(By.CssSelector("md-option"));
            Click(item);
            OKButton().AssertIsEnabled();
        }

        #endregion

        #region Parameter validation

        public virtual void MandatoryParameterEnforced() {
            Debug.WriteLine(nameof(MandatoryParameterEnforced));
            GeminiUrl("home?m1=SalesRepository&d1=FindSalesPersonByName");
            wait.Until(dr => dr.FindElement(By.CssSelector("input#firstname1")).GetAttribute("placeholder") == "");
            wait.Until(dr => dr.FindElement(By.CssSelector("input#lastname1")).GetAttribute("placeholder") == "* ");
            OKButton().AssertIsDisabled().AssertHasTooltip("Missing mandatory fields: Last Name; ");
            ClearFieldThenType("input#lastname1", "a");
            OKButton().AssertIsEnabled();
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Find Sales Person By Name");
        }

        public virtual void AutoCompleteMandatoryParmWithoutSelectionFails() {
            Debug.WriteLine(nameof(AutoCompleteMandatoryParmWithoutSelectionFails));
            //To test  -  enter valid text but don't select from drop-down
            //Assuming parm is mandatory, hitting Ok should give validation message
            GeminiUrl("home?m1=CustomerRepository&d1=FindCustomer");
            ClearFieldThenType("#customer1", "AW000");
            wait.Until(d => d.FindElements(By.CssSelector("ul li a")).Count == 10);
            OKButton().AssertIsDisabled().AssertHasTooltip("Missing mandatory fields: Customer; ");
        }

        public virtual void ValidateSingleValueParameter() {
            Debug.WriteLine(nameof(ValidateSingleValueParameter));
            GeminiUrl("object?o1=___1.Product--342&as1=open&d1=BestSpecialOffer");

            ClearFieldThenType("#quantity1", "0");
            Click(OKButton());
            wait.Until(dr => dr.FindElement(By.CssSelector(".parameter .validation")).Text.Length > 0);
            var validation = WaitForCss(".parameter .validation");
            Assert.AreEqual("Quantity must be > 0", validation.Text);

            var qty = WaitForCss("input#quantity1");
            qty.SendKeys(Keys.Backspace + "1");
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.Object, "No Discount");
        }

        public virtual void ValidateSingleRefParamFromChoices() {
            Debug.WriteLine(nameof(ValidateSingleRefParamFromChoices));
            GeminiUrl("object?o1=___1.SalesOrderHeader--71742&c1_SalesOrderHeaderSalesReason=List&as1=open");

            WaitForView(Pane.Single, PaneType.Object);

            Thread.Sleep(2000);

            var action = wait.Until(dr => dr.FindElement(By.CssSelector("nof-action input[value='Add New Sales Reason']")));

            Click(action);

            wait.Until(dr => dr.FindElement(By.CssSelector("select#reason1")));
            wait.Until(dr => dr.FindElements(By.CssSelector("select#reason1 option")).Count >= 10);
            SelectDropDownOnField("select#reason1", "Price");
            Click(OKButton());
            wait.Until(dr => dr.FindElement(By.CssSelector(".parameter .validation")).Text.Length > 0);
            var validation = WaitForCss(".parameter .validation");
            Assert.AreEqual("Price already exists in Sales Reasons", validation.Text);
        }

        public virtual void CoValidationOfMultipleParameters() {
            Debug.WriteLine(nameof(CoValidationOfMultipleParameters));
            GeminiUrl("object?o1=___1.PurchaseOrderDetail--1632--3660&as1=open&d1=ReceiveGoods");
            wait.Until(dr => dr.FindElement(By.CssSelector("#qtyreceived1")).GetAttribute("value") == "550");
            wait.Until(dr => dr.FindElement(By.CssSelector("#qtyintostock1")).GetAttribute("value") == "550");
            ClearFieldThenType("#qtyreceived1", "100");
            ClearFieldThenType("#qtyrejected1", "50");
            ClearFieldThenType("#qtyintostock1", "49");
            Click(OKButton());
            wait.Until(dr => dr.FindElement(By.CssSelector(".co-validation")).Text ==
                             "Qty Into Stock + Qty Rejected must add up to Qty Received");
        }

        public virtual void OptionalReferenceParamCanBeNull() {
            Debug.WriteLine(nameof(OptionalReferenceParamCanBeNull));
            //Test written against specific bug in 8.0Beta9
            GeminiUrl("home?m1=OrderRepository&d1=FindOrders");
            Click(OKButton());
            WaitForView(Pane.Single, PaneType.List, "Find Orders");
        }

        public virtual void ValidationOfContributeeParameter() {
            Debug.WriteLine(nameof(ValidationOfContributeeParameter));
            GeminiUrl("object?r1=0&i1=View&o1=___1.Customer--10&as1=open&d1=CreateNewOrder");
            Click(OKButton());
            //Test written against #37 where message is preceded by '199 Restful Objects'
            wait.Until(dr => dr.FindElement(By.CssSelector(".co-validation")).Text ==
                             "Customers in Canada may not place orders directly.");
        }

        #endregion
    }

    #region Mega tests

    public abstract class MegaDialogTestsRoot : DialogTestsRoot {
        [TestMethod] //Mega
        [Priority(0)]
        public void DialogTests() {
            PasswordParam();
            ScalarChoicesParm();
            TestCancelDialog();
            FieldsRetainedWhenNavigatingAwayAndBack();
            ReopeningADialogThatWasntCancelledDoesNotRetainFields();
            ScalarParmShowsDefaultValue();
            DateTimeParmKeepsValue();
            TimeSpanParm();
            RefChoicesParmKeepsValue();
            MultipleRefChoicesDefaults();
            MultipleRefChoicesChangeDefaults();
            ConditionalChoices();
            ChoicesDefaults();
            ChoicesOptional();
            ChoicesChangeDefaults();
            ConditionalChoicesDefaults();
            ConditionalChoicesMultiple();
            AutoCompleteParm();
            AutoCompleteParmDefault();
            ClearingAutoCompleteTextClearsUnderlyingReference();
            AutoCompleteParmShowSingleItem();
            AutoCompleteScalarField();
            AutoCompleteOptionalParamNotSelected();
            MandatoryParameterEnforced();
            ValidateSingleValueParameter();
            ValidateSingleRefParamFromChoices();
            CoValidationOfMultipleParameters();
            ParameterDescriptionRenderedAsPlaceholder();
            BooleanParams();
            NullableBooleanParams();
            WarningShownWithinDialogAndInFooter();
            DefaultReferenceParamRendersCorrectly();
            QueryOnlyActionDialogPersists();
            OptionalReferenceParamCanBeNull();
            ValidationOfContributeeParameter();
            NoResultFoundMessageLeavesDialogOpen();
        }

        [TestMethod]
        [Priority(-1)]
        public void ProblematicTests() {
            PotentActionDialogDisappearsAndFieldsNotRemembered();
        }
    }

    //[TestClass]
    public class MegaDialogTestsFirefox : MegaDialogTestsRoot {
        [ClassInitialize]
        public new static void InitialiseClass(TestContext context) {
            GeminiTest.InitialiseClass(context);
        }

        [TestInitialize]
        public virtual void InitializeTest() {
            InitFirefoxDriver();
            Url(BaseUrl);
        }

        [TestCleanup]
        public virtual void CleanupTest() {
            CleanUpTest();
        }
    }

    //[TestClass]
    public class MegaDialogTestsIe : MegaDialogTestsRoot {
        [ClassInitialize]
        public new static void InitialiseClass(TestContext context) {
            FilePath(@"drivers.IEDriverServer.exe");
            GeminiTest.InitialiseClass(context);
        }

        [TestInitialize]
        public virtual void InitializeTest() {
            InitIeDriver();
            Url(BaseUrl);
        }

        [TestCleanup]
        public virtual void CleanupTest() {
            CleanUpTest();
        }
    }

    [TestClass] //toggle
    public class MegaDialogTestsChrome : MegaDialogTestsRoot {
        [ClassInitialize]
        public new static void InitialiseClass(TestContext context) {
            FilePath(@"drivers.chromedriver.exe");
            GeminiTest.InitialiseClass(context);
        }

        [TestInitialize]
        public virtual void InitializeTest() {
            InitChromeDriver();
            Url(BaseUrl);
        }

        [TestCleanup]
        public virtual void CleanupTest() {
            CleanUpTest();
        }
    }

    #endregion
}