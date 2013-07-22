using System.Windows.Controls;

namespace OwnCloud.Extensios
{
    public static class GridExtensions
    {

        public static void SetGridRows(this Grid target, int count)
        {
            target.RowDefinitions.Clear();
            for (int i = 0; i < count; i++)
            {
                target.RowDefinitions.Add(new RowDefinition());
            }
        }

        public static void SetGridColumns(this Grid target, int count)
        {
            target.ColumnDefinitions.Clear();
            for (int i = 0; i < count; i++)
            {
                target.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        public static int GetGridRows(this Grid target)
        {
            return target.RowDefinitions.Count;
        }

    }
}
