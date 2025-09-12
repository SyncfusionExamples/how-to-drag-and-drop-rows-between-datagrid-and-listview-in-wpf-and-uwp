# How to drag and drop rows between datagrid and listview in wpf?
This example illustrates how to drag and drop rows between datagrid and listview in wpf

## Sample
## UWP

```xaml
<Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    
    <Syncfusion:SfDataGrid x:Name="datagrid" 
                            AllowEditing="True"  
                            LiveDataUpdateMode="AllowDataShaping"                                  
                            ColumnSizer="Star"                               
                            AllowDraggingRows="True"  
                            EnableDataVirtualization="True"                                
                            AutoGenerateColumns="False"
                            AllowDrop="True"                              
                            ShowRowHeader="True"
                            ItemsSource="{Binding GDCSource}" >            
        <Syncfusion:SfDataGrid.Columns>
            <Syncfusion:GridTextColumn MappingName="EmployeeName" />
            <Syncfusion:GridTextColumn MappingName="EmployeeAge" />
            <Syncfusion:GridTextColumn MappingName="EmployeeArea" />
            <Syncfusion:GridTextColumn MappingName="EmployeeGender" />
        </Syncfusion:SfDataGrid.Columns>
    </Syncfusion:SfDataGrid>

    <ListView x:Name="listView" Margin="25" CanDrag="True" CanReorderItems="True"
                AllowDrop="True" ItemsSource="{Binding GDCSource1}" 
                SelectionMode="Extended" Grid.Column="1" 
                DisplayMemberPath="EmployeeName" CanDragItems="True">
    </ListView>
</Grid>

C#:

this.datagrid.RowDragDropController = new GridRowDragDropControllerExt();
this.listView.DragItemsStarting += ListView_DragItemsStarting;
this.listView.DragOver += ListView_DragOver;
this.listView.Drop += ListView_Drop;
this.listView.DragEnter += ListView_DragEnter;

private void ListView_DragEnter(object sender, DragEventArgs e)
{
    e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
}

private void ListView_Drop(object sender, DragEventArgs e)
{
    foreach (var item in records1)
    {
        this.datagrid.View.Remove(item as BusinessObjects);

        (this.DataContext as ViewModel).GDCSource1.Add(item as BusinessObjects);
    }
}

ObservableCollection<object> records1 = new ObservableCollection<object>();

private void ListView_DragOver(object sender, DragEventArgs e)
{
    if (e.DataView.Properties.ContainsKey("Records"))
        records1 = e.DataView.Properties["Records"] as ObservableCollection<object>;
}

private void ListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
{
    var records = new ObservableCollection<object>();
    records.Add(listView.SelectedItem);
    e.Data.Properties.Add("DraggedItem", records);
    e.Data.Properties.Add("ListView", listView);
    e.Data.SetText(StandardDataFormats.Text);
}

public class GridRowDragDropControllerExt : GridRowDragDropController
{
    ObservableCollection<object> draggingRecords = new ObservableCollection<object>();

    protected override void ProcessOnDragOver(DragEventArgs args, RowColumnIndex rowColumnIndex)
    {
        if (args.DataView.Properties.ContainsKey("DraggedItem"))
            draggingRecords = args.DataView.Properties["DraggedItem"] as ObservableCollection<object>;

        else
            draggingRecords = args.DataView.Properties["Records"] as ObservableCollection<object>;

        if (draggingRecords == null)
            return;

        var dropPosition = GetDropPosition(args, rowColumnIndex, draggingRecords);

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
        args.AcceptedOperation = DataPackageOperation.Move;
        ShowDragIndicators(dropPosition, rowColumnIndex, args);
        args.Handled = true;
    }

    ListView listview;

    protected override void ProcessOnDrop(DragEventArgs args, RowColumnIndex rowColumnIndex)
    {
        listview = null;
        
        if (args.DataView.Properties.ContainsKey("ListView"))
            listview=args.DataView.Properties["ListView"] as ListView;

        if (!DataGrid.SelectionController.CurrentCellManager.CheckValidationAndEndEdit())
            return;

        var dropPosition = GetDropPosition(args, rowColumnIndex, draggingRecords);
        if (dropPosition == DropPosition.None)
            return;

        var droppingRecordIndex = this.DataGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex);

        if (droppingRecordIndex < 0)
            return;

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
                if (draggingIndex < rowColumnIndex.RowIndex && dropPosition == DropPosition.DropAbove)
                    this.DataGrid.View.Records.Insert(droppingRecordIndex - 1, this.DataGrid.View.Records.CreateRecord(record));
                else if (draggingIndex > rowColumnIndex.RowIndex && dropPosition == DropPosition.DropBelow)
                    this.DataGrid.View.Records.Insert(droppingRecordIndex + 1, this.DataGrid.View.Records.CreateRecord(record));
                else
                    this.DataGrid.View.Records.Insert(droppingRecordIndex, this.DataGrid.View.Records.CreateRecord(record));
            }
        }
        CloseDragIndicators();
    }
}
```

## WPF

```xaml
<Window x:Class="RowDragAndDropBetweenControlsDemo.MainWindow"
                             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                             xmlns:local="clr-namespace:RowDragAndDropBetweenControlsDemo"
                             xmlns:syncfusion="http://schemas.syncfusion.com/wpf"
                             xmlns:behavior="http://schemas.microsoft.com/xaml/behaviors"
                             Title="Drag and Drop" Height="710" Width="1024" 
                             WindowStartupLocation="CenterScreen"
                             Icon="App.ico" >

    <Window.DataContext>
        <local:ViewModel/>
    </Window.DataContext>
    <Window.Resources>
        <DataTemplate x:Key="template">
            <code>
            . . .
            . . .
            <code>
        </DataTemplate>
    </Window.Resources>

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="650"/>
            <ColumnDefinition Width="250"/>
        </Grid.ColumnDefinitions>
        <syncfusion:SfDataGrid x:Name="sfDataGrid"
                               AllowDrop="True"
                               AllowDraggingRows="True" 
                               AllowSorting="True" 
                               AutoGenerateColumns="False"
                               ColumnSizer="AutoLastColumnFill"   
                               SelectionMode="Single" 
                               RowDragDropTemplate="{StaticResource template}"
                               ItemsSource="{Binding OrderDetails}"
                               ShowRowHeader="True" Margin="10,19,240,169" Grid.ColumnSpan="2">
            <syncfusion:SfDataGrid.Columns>
                <syncfusion:GridTextColumn HeaderText="Order ID" MappingName="OrderID"/>
                <syncfusion:GridTextColumn HeaderText="Customer ID" MappingName="CustomerID" />
                <syncfusion:GridTextColumn HeaderText="Ship Name" MappingName="ShipName" />
                <syncfusion:GridTextColumn HeaderText="Ship City" MappingName="ShipCity" />
                <syncfusion:GridTextColumn HeaderText="Ship Address" MappingName="ShipAddress" />
            </syncfusion:SfDataGrid.Columns>
        </syncfusion:SfDataGrid>

        <ListView x:Name="listView" AllowDrop="True" Margin="30,19,0,169"
                               ItemsSource="{Binding OrderDetails1}"
                               Grid.Column="1" DisplayMemberPath="ShipName" >
        </ListView>

    </Grid>
    <behavior:Interaction.Behaviors>
        <local:DragDropBehavior/>
    </behavior:Interaction.Behaviors>
</Window>

C#:

AssociatedObject.sfDataGrid.RowDragDropController.Drop += RowDragDropController_Drop;
AssociatedObject.listView.PreviewMouseMove += ListView_PreviewMouseMove;
AssociatedObject.listView.Drop += ListView_Drop;

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
```