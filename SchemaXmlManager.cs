namespace Bau.Libraries.LibDbSchema.Repository.Xml;

/// <summary>
///		Manager para el almacenamiento de esquemas en XML
/// </summary>
public class SchemaXmlManager
{
	/// <summary>
	///		Carga un esquema de un archivo
	/// </summary>
	public LibDbProviders.Base.Schema.SchemaDbModel Load(string fileName) => new Repositories.SchemaRepository().Load(fileName);

	/// <summary>
	///		Graba un esquema en un archivo
	/// </summary>
	public void Save(LibDbProviders.Base.Schema.SchemaDbModel schema, string fileName)
	{
		new Repositories.SchemaRepository().Save(schema, fileName);
	}
}
