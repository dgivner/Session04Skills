#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

#endregion

namespace Session04Skills
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            IList<Element> pickList = uidoc.Selection.PickElementsByRectangle("Select elements");

            List<CurveElement> lineList = new List<CurveElement>();

            foreach (Element element in pickList)
            {
                if (element is CurveElement)
                {
                    CurveElement curve = element as CurveElement;

                    if(curve.CurveElementType == CurveElementType.ModelCurve)
                        lineList.Add(curve);
                }
            }

            Transaction t = new Transaction(doc);
            t.Start("Create Storefront Wall");

            Level newLevel = Level.Create(doc, 15);
            WallType currentWT = GetWallTypeByName(doc, "Storefront");
            MEPSystemType pipeSystemType = GetMEPSystemTypeByName(doc, "Domestic Hot Water");
            PipeType pipeType = GetPipeTypeByName(doc, "Default");

            MEPSystemType ductSystemType = GetMEPSystemTypeByName(doc, "Supply Air");
            DuctType ductType = GetDuctTypeByName(doc, "Default");

            //This is for the wall type Generic 8"
            WallType currentWT2 = GetWallTypeByName(doc, "Generic - 8");

            //This is for the currentDT Default
            DuctType currentDT = GetDuctTypeByName(doc, "Default");

            //This is for the currentPT Default
            PipeType currentPT = GetPipeTypeByName(doc, "Default");


            foreach (CurveElement currentCurve in lineList)
            {
                GraphicsStyle currentGS = currentCurve.LineStyle as GraphicsStyle;
                Debug.Print(currentGS.Name);

                Curve curve = currentCurve.GeometryCurve;
                XYZ startPoint = curve.GetEndPoint(0);
                XYZ endPoint = curve.GetEndPoint(1);


                switch (currentGS.Name)
                {
                    case "M-DUCT":
                        TaskDialog.Show("Duct", "The line is M-DUCT");
                        Duct newDuct = Duct.Create(doc, ductSystemType.Id, ductType.Id, newLevel.Id, startPoint, endPoint);
                        break;

                    case "A-WALL":
                        TaskDialog.Show("Generic Wall", "This line is A-WALL");
                        Wall newWall1 = Wall.Create(doc, curve, currentWT2.Id, newLevel.Id, 20, 0, false, false);
                        break;

                    case "A-GLAZ":
                        TaskDialog.Show("Storefront Wall", "This line is A-GLAZ");
                        Wall newWall2 = Wall.Create(doc, curve, currentWT.Id, newLevel.Id, 20, 0, false, false);
                        break;

                    case "P-PIPE":
                        Pipe newPipe = Pipe.Create(doc, pipeSystemType.Id, pipeType.Id, newLevel.Id, startPoint, endPoint);
                        break;
                }

            }

            t.Commit();
            t.Dispose();

            TaskDialog.Show("Test", "I have" + lineList.Count + "lines.");



            return Result.Succeeded;
        }

        
        private WallType GetWallTypeByName(Document doc, string wallType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (WallType currentWT in collector)
            {
                if (currentWT.Name == wallType)
                    return currentWT;
            }

            return null;
        }

        private DuctType GetDuctTypeByName(Document doc, string ductType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DuctType));

            foreach (DuctType currentDT in collector)
            {
                if (currentDT.Name == ductType)
                    return currentDT;
            }

            return null;
        }

        private PipeType GetPipeTypeByName(Document doc, string pipeType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (PipeType currentPT in collector)
            {
                if (currentPT.Name == pipeType)
                    return currentPT;
            }

            return null;
        }
        private MEPSystemType GetMEPSystemTypeByName(Document doc, string wallType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));

            foreach (MEPSystemType currentST in collector)
            {
                if (currentST.Name == wallType)
                    return currentST;
            }

            return null;
        }
    }
}
