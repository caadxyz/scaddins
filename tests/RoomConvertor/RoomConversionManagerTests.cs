﻿using Autodesk.Revit.DB;
using NUnit.Framework;
using RTF.Applications;
using RTF.Framework;
using SCaddins.RoomConverter;

namespace SCaddins.Tests.RoomConvertor
{
    [TestFixture()]
    public class RoomConversionManagerTests
    {
        [Test()]
        [TestModel(@"./scaddins_test_model.rvt")]
        public void CreateViewsAndSheetsTest()
        {
            var doc = RevitTestExecutive.CommandData.Application.ActiveUIDocument.Document;
            var origViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .ToElements().Count;
            var manager = new RoomConversionManager(doc);
            manager.CreateViewsAndSheets(manager.Candidates);
            var newViews = new FilteredElementCollector(doc)
    .           OfClass(typeof(View))
                .ToElements().Count;
            Assert.IsTrue(newViews - origViews == 16);
        }

        [Test()]
        [TestModel(@"./scaddins_test_model.rvt")]
        public void GetAllTitleBlockTypesTest()
        {
            var doc = RevitTestExecutive.CommandData.Application.ActiveUIDocument.Document;
            // Allow for an empty title block in this count
            Assert.IsTrue(RoomConversionManager.GetAllTitleBlockTypes(doc).Count == 5);
        }

        [Test()]
        [TestModel(@"./scaddins_test_model.rvt")]
        public void GetTitleBlockByNameTest()
        {
            var doc = RevitTestExecutive.CommandData.Application.ActiveUIDocument.Document;
            var manager = new RoomConversionManager(doc);
            Assert.IsTrue(manager.GetTitleBlockByName("A0 metric-A0 metric") != ElementId.InvalidElementId);
            Assert.IsTrue(manager.GetTitleBlockByName("a2-Family3") != ElementId.InvalidElementId);
            //Assert.IsTrue(manager.GetTitleBlockByName(null) == ElementId.InvalidElementId);
            Assert.IsTrue(manager.GetTitleBlockByName(@"!@#$%^&*(){}[]") == ElementId.InvalidElementId);
        }

        [Test()]
        [TestModel(@"./scaddins_test_model.rvt")]
        public void CreateRoomMassesTest()
        {
            var doc = RevitTestExecutive.CommandData.Application.ActiveUIDocument.Document;
            var origMasses = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Mass)
                .OfClass(typeof(DirectShape))
                .ToElements().Count;
            var manager = new RoomConversionManager(doc);
            manager.CreateRoomMasses(manager.Candidates);
            var fec = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Mass).OfClass(typeof(DirectShape));
            var newMasses = fec.ToElements().Count - origMasses;
            Assert.IsTrue(newMasses == 8);
        }

        [SetUp]
        public void Setup()
        {
            SCaddinsApp.WindowManager = new SCaddins.Common.WindowManager(new SCaddins.Common.MockDialogService());
        }

        [Test()]
        [TestModel(@"./scaddins_test_model.rvt")]
        public void SynchronizeMassesToRoomsTest()
        {
            var doc = RevitTestExecutive.CommandData.Application.ActiveUIDocument.Document;
            var manager = new RoomConversionManager(doc);
            manager.CreateRoomMasses(manager.Candidates);
            var fec = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Mass).OfClass(typeof(DirectShape));
            foreach (Element e in fec) {
                var param = e.LookupParameter("Department");
                if (param != null && param.AsString() != "Test") {
                    param.Set("Test");
                } else {
                    Assert.Fail();
                }
            }
            manager.SynchronizeMassesToRooms();
            //TIDO
            Assert.Fail();
        }
    }
}