using Microsoft.Xaml.Behaviors;
using RowDragAndDropBetweenControlsDemo.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace RowDragAndDropBetweenControlsDemo
{
    public class DragDropBehavior : Behavior<MainWindow>
    {
        protected override void OnAttached()
        {
            AssociatedObject.sfDataGrid.RowDragDropController.Drop += RowDragDropController_Drop;
            AssociatedObject.listView.PreviewMouseMove += ListView_PreviewMouseMove;
            AssociatedObject.listView.Drop += ListView_Drop;
            base.OnAttached();
        }

        /// <summary>
        /// To add the dropped records in the ListView control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_Drop(object sender, DragEventArgs e)
        {
            ObservableCollection<object> DraggingRecords = new ObservableCollection<object>();
            if (e.Data.GetDataPresent("ListViewRecords"))
            {
                DraggingRecords = e.Data.GetData("ListViewRecords") as ObservableCollection<object>;

                var listViewRecord = DraggingRecords[0] as Orders;

                (AssociatedObject.listView.ItemsSource as ObservableCollection<Orders>).Remove(listViewRecord);
                (this.AssociatedObject.DataContext as ViewModel).OrderDetails1.Add(listViewRecord);
            }
            else
            {
                DraggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;

                var record = DraggingRecords[0] as Orders;

                this.AssociatedObject.sfDataGrid.View.Remove(record);
                (this.AssociatedObject.DataContext as ViewModel).OrderDetails1.Add(record);
            }
        }

      
        private void ListView_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBox dragSource = null;

                var records = new ObservableCollection<object>();

                ListBox parent = (ListBox)sender;

                dragSource = parent;

                object data = GetDataFromListBox(dragSource, e.GetPosition(parent));

                records.Add(data);

                var dataObject = new DataObject();
                dataObject.SetData("ListViewRecords", records);
                dataObject.SetData("ListView", AssociatedObject.listView);

                if (data != null)
                {
                    DragDrop.DoDragDrop(parent, dataObject, DragDropEffects.Move);
                }

            }
            e.Handled = true;
        }

        /// <summary>
        /// Get the data from list box control
        /// </summary>
        /// <param name="source"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);
                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }
                    if (element == source)
                    {
                        return null;
                    }
                }
                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }
            return null;
        }

        private void RowDragDropController_Drop(object sender, Syncfusion.UI.Xaml.Grid.GridRowDropEventArgs e)
        {
            if (e.IsFromOutSideSource)
            {
                ObservableCollection<object> DraggingRecords = new ObservableCollection<object>();
                if (e.Data.GetDataPresent("ListViewRecords"))
                    DraggingRecords = e.Data.GetData("ListViewRecords") as ObservableCollection<object>;
                else
                    DraggingRecords = e.Data.GetData("Records") as ObservableCollection<object>;

                var draggingRecords = DraggingRecords[0] as Orders;

                int dropIndex = (int)e.TargetRecord;

                var dropPosition = e.DropPosition.ToString();

                IList collection = AssociatedObject.sfDataGrid.View.SourceCollection as IList;

                if (dropPosition == "DropAbove")
                {
                    dropIndex--;
                    collection.Insert(dropIndex, draggingRecords);
                }
                else
                {
                    dropIndex++;
                    collection.Insert(dropIndex, draggingRecords);
                }

                (AssociatedObject.listView.ItemsSource as ObservableCollection<Orders>).Remove(draggingRecords as Orders);
                e.Handled = true;

            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.sfDataGrid.RowDragDropController.Drop -= RowDragDropController_Drop;
            AssociatedObject.listView.PreviewMouseMove -= ListView_PreviewMouseMove;
            AssociatedObject.listView.Drop -= ListView_Drop;
            base.OnDetaching();
        }
    }
}
