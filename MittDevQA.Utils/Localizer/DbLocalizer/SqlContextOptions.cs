namespace Utils.Localizer.DbLocalizer
{
    public class SqlContextOptions
    {
        /// <summary>
        /// SQL Server schema on which the tables are supposed to be created, if none, database default will be used
        /// </summary>
        public string SqlSchemaName { get; set; }
    }
}