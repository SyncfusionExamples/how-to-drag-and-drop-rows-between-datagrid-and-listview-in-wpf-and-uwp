using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Syncfusion.UI.Xaml.ScrollAxis;
using Windows.ApplicationModel.DataTransfer;
using System.Collections;
using Windows.UI.Xaml.Media.Imaging;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid.Helpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ListViewDragDropDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.datagrid.RowDragDropController = new GridRowDragDropControllerExt();
            this.listView.DragItemsStarting += ListView_DragItemsStarting;
            this.listView.DragOver += ListView_DragOver;
            this.listView.Drop += ListView_Drop;
            this.listView.DragEnter += ListView_DragEnter;
        }

    /// <summary>
    /// to dragged and dropped the selected records in ListView control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListView_DragEnter(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
    }

    /// <summary>
    /// to add the dropped record in the ListView control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListView_Drop(object sender, DragEventArgs e)
    {
        foreach (var item in records1)
        {
            this.datagrid.View.Remove(item as BusinessObjects);

            (this.DataContext as ViewModel).GDCSource1.Add(item as BusinessObjects);
        }
    }

    ObservableCollection<object> records1 = new ObservableCollection<object>();

    /// <summary>
    /// to move the dragged items form the ListView control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListView_DragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Properties.ContainsKey("Records"))
            records1 = e.DataView.Properties["Records"] as ObservableCollection<object>;
    }

    /// <summary>
    /// to select and dragging the records 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
        var records = new ObservableCollection<object>();
        records.Add(listView.SelectedItem);
        e.Data.Properties.Add("DraggedItem", records);
        e.Data.Properties.Add("ListView", listView);
        e.Data.SetText(StandardDataFormats.Text);
    }
   }

    public class GridRowDragDropControllerExt : GridRowDragDropController
    {
        ObservableCollection<object> draggingRecords = new ObservableCollection<object>();

        /// <summary>
        /// Occurs when the input system reports an underlying dragover event with this element as the potential drop target.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        protected override void ProcessOnDragOver(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (args.DataView.Properties.ContainsKey("DraggedItem"))
                draggingRecords = args.DataView.Properties["DraggedItem"] as ObservableCollection<object>;

            else

                draggingRecords = args.DataView.Properties["Records"] as ObservableCollection<object>;

            if (draggingRecords == null)
                return;

            //To get the drop position of the record
            var dropPosition = GetDropPosition(args, rowColumnIndex, draggingRecords);

            // based on drop positon, the popup will be shown
            if (dropPosition == DropPosition.None)
            {
                CloseDragIndicators();
                args.AcceptedOperation = DataPackageOperation.None;
                args.DragUIOverride.Caption = "Can't drop here";
                return;
            }

            else if (dropPosition == DropPosition.DropAbove)
            {
                if (draggingRecords != null && draggingRecords.Count > 1)
                    args.DragUIOverride.Caption = "Drop these " + draggingRecords.Count + "  rows above";
                else
                {
                    args.AcceptedOperation = DataPackageOperation.Copy;

                    args.DragUIOverride.IsCaptionVisible = true;
                    args.DragUIOverride.IsContentVisible = true;
                    args.DragUIOverride.IsGlyphVisible = true;
                    args.DragUIOverride.Caption = "Drop above";
                }
            }
          
            else
            {
                if (draggingRecords != null && draggingRecords.Count > 1)
                    args.DragUIOverride.Caption = "Drop these " + draggingRecords.Count + "  rows below";
                else
                    args.DragUIOverride.Caption = "Drop below";
            }
            // to accept and move the records. 
            args.AcceptedOperation = DataPackageOperation.Move;

            //To Show the up and down indicators while dragging the row
            ShowDragIndicators(dropPosition, rowColumnIndex, args);
            args.Handled = true;
        }

        ListView listview;

        /// <summary>
        /// Occurs when the input system reports an underlying drop event with this element as the drop target.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        protected override void ProcessOnDrop(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            listview = null;
            
            if (args.DataView.Properties.ContainsKey("ListView"))
                listview=args.DataView.Properties["ListView"] as ListView;

            if (!DataGrid.SelectionController.CurrentCellManager.CheckValidationAndEndEdit())
                return;

            //To get the drop position of the record
            var dropPosition = GetDropPosition(args, rowColumnIndex, draggingRecords);
            // based on drop positon, the popup will be shown
            if (dropPosition == DropPosition.None)
                return;

            var droppingRecordIndex = this.DataGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex);

            if (droppingRecordIndex < 0)
                return;

            // to insert the dragged records based on dropping records index 
            foreach (var record in draggingRecords)
            {
                if (listview != null)
                {
                    (listview.ItemsSource as ObservableCollection<BusinessObjects>).Remove(record as BusinessObjects);
                    var sourceCollection = this.DataGrid.View.SourceCollection as IList;

                    if (dropPosition == DropPosition.DropBelow)
                        sourceCollection.Insert(droppingRecordIndex + 1, record);
                    else
                        sourceCollection.Insert(droppingRecordIndex, record);
                }
                else
                {
                    var draggingIndex = this.DataGrid.ResolveToRowIndex(draggingRecords[0]);

                    if (draggingIndex < 0)
                    {
                        return;
                    }

                    var recordindex = this.DataGrid.ResolveToRecordIndex(draggingIndex);
                    var recordEntry = this.DataGrid.View.Records[recordindex];
                    this.DataGrid.View.Records.Remove(recordEntry);
                    // to insert the dragged records based on dropping records index 
                    if (draggingIndex < rowColumnIndex.RowIndex && dropPosition == DropPosition.DropAbove)
                        this.DataGrid.View.Records.Insert(droppingRecordIndex - 1, this.DataGrid.View.Records.CreateRecord(record));
                    else if (draggingIndex > rowColumnIndex.RowIndex && dropPosition == DropPosition.DropBelow)
                        this.DataGrid.View.Records.Insert(droppingRecordIndex + 1, this.DataGrid.View.Records.CreateRecord(record));
                    else
                        this.DataGrid.View.Records.Insert(droppingRecordIndex, this.DataGrid.View.Records.CreateRecord(record));
                }
            }
            //Closes the Drag arrow indication all the rows
            CloseDragIndicators();
        }
    }
}
    

                                            