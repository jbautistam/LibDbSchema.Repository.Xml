using Bau.Libraries.LibHelper.Extensors;
using Bau.Libraries.LibMarkupLanguage;
using Bau.Libraries.LibDbProviders.Base.Schema;

namespace Bau.Libraries.LibDbSchema.Repository.Xml.Repositories;

/// <summary>
///		Repositorio de un esquema de base de datos en XML
/// </summary>
internal class SchemaRepository
{
	// Constantes privadas
	private const string TagRoot = "Schema";
	private const string TagTable = "Table";
	private const string TagView = "View";
	private const string TagSchema = "Schema";
	private const string TagCatalog = "Catalog";
	private const string TagName = "Name";
	private const string TagDescription = "Description";
	private const string TagCreatedAt = "CreatedAt";
	private const string TagUpdateAt = "UpdateAt";
	private const string TagDefinition = "Definition";
	private const string TagCheckOption = "CheckOption";
	private const string TagUpdatable = "Updatable";
	private const string TagField = "Field";
	private const string TagType = "Type";
	private const string TagDbType = "DbType";
	private const string TagIsKey = "IsKey";
	private const string TagLength = "Length";
	private const string TagRequired = "Required";
	private const string TagFormat = "Format";
	private const string TagPosition = "Position";
	private const string TagDefault = "Default";
	private const string TagIdentity = "Identity";
	private const string TagConstraint = "Constraint";

	/// <summary>
	///		Carga los datos de un esquema de un archivo
	/// </summary>
	internal SchemaDbModel Load(string fileName)
	{
		SchemaDbModel schema = new SchemaDbModel();

			// Carga los datos
			if (File.Exists(fileName))
			{
				MLFile fileML = new LibMarkupLanguage.Services.XML.XMLParser().Load(fileName);

					foreach (MLNode rootML in fileML.Nodes)
						if (rootML.Name == TagRoot)
							foreach (MLNode nodeML in rootML.Nodes)
								switch (nodeML.Name)
								{
									case TagTable:
											schema.Tables.Add(LoadTable(nodeML));
										break;
									case TagView:
											schema.Views.Add(LoadView(nodeML));
										break;
								}
			}
			// Devuelve los datos del esquema
			return schema;
	}

	/// <summary>
	///		Carga los datos de una tabla
	/// </summary>
	private TableDbModel LoadTable(MLNode rootML)
	{
		TableDbModel table = new TableDbModel();

			// Asigna las propiedades básicas
			AssignBaseProperties(rootML, table);
			// Carga los campos
			table.Fields.AddRange(LoadFields(rootML));
			table.Constraints.AddRange(LoadConstraints(rootML));
			// Devuelve la tabla
			return table;
	}

	/// <summary>
	///		Carga los datos de una vista
	/// </summary>
	private ViewDbModel LoadView(MLNode rootML)
	{
		ViewDbModel view = new ViewDbModel();

			// Asigna las propiedades básicas
			AssignBaseProperties(rootML, view);
			// Asigna el resto de propiedades
			view.Definition = rootML.Nodes[TagDefinition].Value.TrimIgnoreNull();
			view.CheckOption = rootML.Attributes[TagCheckOption].Value.TrimIgnoreNull();
			view.IsUpdatable = rootML.Attributes[TagUpdatable].Value.GetBool();
			// Carga los campos
			view.Fields.AddRange(LoadFields(rootML));
			// Devuelve la tabla
			return view;
	}

	/// <summary>
	///		Carga los campos de un nodo
	/// </summary>
	private List<FieldDbModel> LoadFields(MLNode rootML)
	{
		List<FieldDbModel> fields = new List<FieldDbModel>();

			// Carga los campos
			foreach (MLNode nodeML in rootML.Nodes)
				if (nodeML.Name == TagField)
				{
					FieldDbModel field = new FieldDbModel();

						// Asigna las propiedades básicas
						AssignBaseProperties(nodeML, field);
						// Carga los datos del campo
						field.Table = nodeML.Attributes[TagTable].Value.TrimIgnoreNull();
						field.Type = nodeML.Attributes[TagType].Value.GetEnum(FieldDbModel.Fieldtype.Unknown);
						field.DbType = nodeML.Attributes[TagDbType].Value.TrimIgnoreNull();
						field.IsKey = nodeML.Attributes[TagIsKey].Value.GetBool();
						field.Length = nodeML.Attributes[TagLength].Value.GetInt(0);
						field.IsRequired = nodeML.Attributes[TagRequired].Value.GetBool();
						field.Format = nodeML.Attributes[TagFormat].Value.TrimIgnoreNull();
						field.OrdinalPosition = nodeML.Attributes[TagPosition].Value.GetInt(0);
						field.Default = nodeML.Nodes[TagDefault].Value.TrimIgnoreNull();
						field.IsIdentity = nodeML.Attributes[TagIdentity].Value.GetBool();
						// Añade el campo a la colección
						fields.Add(field);
				}
			// Devuelve la colección de campos
			return fields;
	}

	/// <summary>
	///		Carga las restricciones
	/// </summary>
	private List<ConstraintDbModel> LoadConstraints(MLNode rootML)
	{
		List<ConstraintDbModel> constraints = new List<ConstraintDbModel>();

			// Carga las restricciones
			foreach (MLNode nodeML in rootML.Nodes)
				if (nodeML.Name == TagConstraint)
				{
					ConstraintDbModel constraint = new ConstraintDbModel();

						// Asigna los datos básicos
						AssignBaseProperties(nodeML, constraint);
						// Asigna los datos a la restricción
						constraint.Table = rootML.Attributes[TagTable].Value.TrimIgnoreNull();
						constraint.Column = rootML.Attributes[TagField].Value.TrimIgnoreNull();
						constraint.Type = rootML.Attributes[TagType].Value.GetEnum(ConstraintDbModel.ConstraintType.Unknown);
						constraint.Position = rootML.Attributes[TagPosition].Value.GetInt(0);
						// Añade la restriccion a la colección
						constraints.Add(constraint);
				}
			// Devuelve la colección
			return constraints;
	}

	/// <summary>
	///		Asigna las propiedades básicas del nodo al objeto de esquema
	/// </summary>
	private void AssignBaseProperties(MLNode rootML, BaseSchemaDbModel dbModel)
	{
		dbModel.Schema = rootML.Attributes[TagSchema].Value.TrimIgnoreNull();
		dbModel.Catalog = rootML.Attributes[TagCatalog].Value.TrimIgnoreNull();
		dbModel.Name = rootML.Attributes[TagName].Value.TrimIgnoreNull();
		dbModel.Description = rootML.Nodes[TagDescription].Value.TrimIgnoreNull();
		dbModel.CreatedAt = rootML.Attributes[TagCreatedAt].Value.GetDateTime(DateTime.Now);
		dbModel.UpdatedAt = rootML.Attributes[TagUpdateAt].Value.GetDateTime(DateTime.Now);
	}

	/// <summary>
	///		Graba los datos de un esquema
	/// </summary>
	internal void Save(SchemaDbModel schema, string fileName)
	{
		MLFile fileML = new MLFile();
		MLNode rootML = fileML.Nodes.Add(TagRoot);

			// Añade los datos
			rootML.Nodes.AddRange(GetNodesTables(schema.Tables));
			rootML.Nodes.AddRange(GetNodesViews(schema.Views));
			// Graba el archivo
			new LibMarkupLanguage.Services.XML.XMLWriter().Save(fileName, fileML);
	}

	/// <summary>
	///		Obtiene los nodos de las tablas
	/// </summary>
	private MLNodesCollection GetNodesTables(List<TableDbModel> tables)
	{
		MLNodesCollection nodes = new MLNodesCollection();

			// Añade los nodos de las tablas
			foreach (TableDbModel table in tables)
			{
				MLNode nodeML = GetNodeBase(TagTable, table);

					// Añade los nodos hijo
					nodeML.Nodes.AddRange(GetNodesFields(table.Fields));
					nodeML.Nodes.AddRange(GetNodesConstraints(table.Constraints));
					// Añade el nodo a la colección
					nodes.Add(nodeML);
			}
			// Devuelve la colección de nodos
			return nodes;
	}

	/// <summary>
	///		Obtiene los nodos de las vistas
	/// </summary>
	private MLNodesCollection GetNodesViews(List<ViewDbModel> views)
	{
		MLNodesCollection nodes = new MLNodesCollection();

			// Genera la colección de nodos
			foreach (ViewDbModel view in views)
			{
				MLNode nodeML = GetNodeBase(TagView, view);

					// Añade los atributos de la vista
					nodeML.Nodes.AddIfNotEmpty(TagDefinition, view.Definition);
					nodeML.Attributes.AddIfNotEmpty(TagCheckOption, view.CheckOption);
					nodeML.Attributes.Add(TagUpdatable, view.IsUpdatable);
					// Añade los nodos de los campos
					nodeML.Nodes.AddRange(GetNodesFields(view.Fields));
					// Añade el nodo a la colección
					nodes.Add(nodeML);
			}
			// Devuelve los nodos
			return nodes;
	}

	/// <summary>
	///		Obtiene la colección de nodos de los campos
	/// </summary>
	private MLNodesCollection GetNodesFields(List<FieldDbModel> fields)
	{
		MLNodesCollection nodes = new MLNodesCollection();

			// Añade los nodos de los campos
			foreach (FieldDbModel field in fields)
			{
				MLNode nodeML = GetNodeBase(TagField, field);

					// Añade los datos del campo
					nodeML.Attributes.AddIfNotEmpty(TagTable, field.Table);
					nodeML.Attributes.AddIfNotEmpty(TagType, field.Type.ToString());
					nodeML.Attributes.AddIfNotEmpty(TagDbType, field.DbType);
					nodeML.Attributes.Add(TagIsKey, field.IsKey);
					nodeML.Attributes.AddIfNotEmpty(TagLength, field.Length);
					nodeML.Attributes.Add(TagRequired, field.IsRequired);
					nodeML.Attributes.AddIfNotEmpty(TagFormat, field.Format);
					nodeML.Attributes.AddIfNotEmpty(TagPosition, field.OrdinalPosition);
					nodeML.Nodes.AddIfNotEmpty(TagDefault, field.Default);
					nodeML.Attributes.Add(TagIdentity, field.IsIdentity);
					// Añade el nodo
					nodes.Add(nodeML);
			}
			// Devuelve la colección de nodos
			return nodes;
	}

	/// <summary>
	///		Obtiene la colección de nodos de las restricciones
	/// </summary>
	private MLNodesCollection GetNodesConstraints(List<ConstraintDbModel> constraints)
	{
		MLNodesCollection nodes = new MLNodesCollection();

			// Añade los nodos de las restricciones
			foreach (ConstraintDbModel constraint in constraints)
			{
				MLNode nodeML = GetNodeBase(TagConstraint, constraint);

					// Asigna los datos a la restricción
					nodeML.Attributes.AddIfNotEmpty(TagTable, constraint.Table);
					nodeML.Attributes.AddIfNotEmpty(TagField, constraint.Column);
					nodeML.Attributes.AddIfNotEmpty(TagType, constraint.Type.ToString());
					nodeML.Attributes.AddIfNotEmpty(TagPosition, constraint.Position);
					// Añade el nodo a la colección
					nodes.Add(nodeML);
			}
			// Devuelve la colección
			return nodes;
	}

	/// <summary>
	///		Obtiene un nodo con los datos básicos de un objeto
	/// </summary>
	private MLNode GetNodeBase(string tag, BaseSchemaDbModel dbModel)
	{
		MLNode rootML = new MLNode(tag);

			// Añade las propiedades
			rootML.Attributes.AddIfNotEmpty(TagSchema, dbModel.Schema);
			rootML.Attributes.AddIfNotEmpty(TagCatalog, dbModel.Catalog);
			rootML.Attributes.AddIfNotEmpty(TagName, dbModel.Name);
			rootML.Nodes.AddIfNotEmpty(TagDescription, dbModel.Description);
			rootML.Attributes.AddIfNotEmpty(TagCreatedAt, dbModel.CreatedAt);
			rootML.Attributes.AddIfNotEmpty(TagUpdateAt, dbModel.UpdatedAt);
			// Devuelve el nodo
			return rootML;
	}
}
