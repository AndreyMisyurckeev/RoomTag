using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomTag
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;

                FilteredElementCollector rooms = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms);
                IList<ElementId> roomids = rooms.ToElementIds() as IList<ElementId>;

                List<ViewPlan> views = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewPlan))
                    .OfType<ViewPlan>()
                    .ToList();

                Transaction transaction = new Transaction(doc, "Номер помещения");
                transaction.Start();

                foreach (View view in views)
                {
                    foreach (ElementId roomid in roomids)
                    {
                        Element element = doc.GetElement(roomid);
                        Room room = element as Room;
                        XYZ roomCenter = GetElementCenter(room);
                        UV center = new UV(roomCenter.X, roomCenter.Y);
                        doc.Create.NewRoomTag(new LinkElementId(roomid), center, view.Id);
                    }
                }
                transaction.Commit();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null);
            return (bounding.Max + bounding.Min) / 2;
        }
    }
}


