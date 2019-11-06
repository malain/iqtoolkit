using Hopex.Core;
using Hopex.Core.Adapters;
using Hopex.Core.Domain;
using IQToolkit.Data;
using IQToolkit.Data.Common;
using IQToolkit.Data.Mapping;
using IQToolkit.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.Json;

namespace ConsoleApp1
{
    class HopexLanguage : TSqlLanguage
    {
        public HopexLanguage() : base()
        {

        }

        public override QueryLinguist CreateLinguist(QueryTranslator translator)
        {
            return new HopexLinguist(this, translator);
        }

        class HopexLinguist : QueryLinguist
        {
            public HopexLinguist(TSqlLanguage language, QueryTranslator translator)
                : base(language, translator)
            {
            }


            public override string Format(Expression expression, bool isHopexModelElement)
            {
                //var isHopexElement = this.Translator.Mapper.Mapping.GetEntity()
                return HopexFormatter.Format(expression, this.Language, isHopexModelElement);
            }
        }
        private static HopexLanguage _default;

        public new static HopexLanguage Default
        {
            get
            {
                if (_default == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _default, new HopexLanguage(), null);
                }
                return _default;
            }
        }
    }

    public class HopexFormatter : TSqlFormatter
    {
        private readonly bool _isHopexModelElement;
        private readonly MappingEntity _entity;

        protected HopexFormatter(QueryLanguage language, bool isHopexModelElement, MappingEntity entity)
            : base(language)
        {
            _isHopexModelElement = isHopexModelElement;
            _entity = entity;
        }

        public static string Format(Expression expression, QueryLanguage language, bool isHopexModelElement)
        {
            var tableExpression = (expression as SelectExpression).From as TableExpression;
            var formatter = new HopexFormatter(language, isHopexModelElement, tableExpression?.Entity);
            formatter.Visit(expression);
            return formatter.ToString();
        }


        protected override void WriteColumns(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            if (columns.Count > 0 && _isHopexModelElement)
            {
                var c = (ColumnExpression)columns[0].Expression;
                var alias = this.GetAliasName(c.Alias);
                this.Write($"{alias}.[Id], {alias}.[Modified], {alias}.[Data]");
            }
            else
            {
                base.WriteColumns(columns);
            }
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            this.Write("JSON_VALUE(");

            if (column.Alias != null && !this.HideColumnAliases)
            {
                this.WriteAliasName(GetAliasName(column.Alias));
                this.Write(".");
            }

            this.Write($"[Data], '$.{column.Name}')");
            return column;
        }

        protected override Expression VisitPredicate(Expression expr)
        {
            if (_entity != null)
            {
                this.Write($"[DomainName]='MySchema' AND [SchemaId]='0010:191105051441431IAJ' AND ");
            }
            return base.VisitPredicate(expr);
        }

        protected override void WriteTableName(string tableName)
        {
            this.Write("[Test].[dbo].[HopexData]");
        }
    }

    public class HopexMapping: AttributeMapping
    {
        public HopexMapping(): base(null)
        {
        }

        public override string GetEntityId(Type entityType)
        {
            return base.GetEntityId(entityType);
        }

        //public override MappingEntity GetEntity(Type entityType, string entityId)
        //{
        //    return this.GetEntity(entityType, entityId ?? this.GetEntityId(entityType), null);
        //}
    }

    public class HopexQueryProvider : SqlQueryProvider
    {
        private readonly IHopexUnitOfWork _context;

        /// <summary>
        /// Constructs a <see cref="SqlQueryProvider"/>
        /// </summary>
        public HopexQueryProvider(IHopexUnitOfWork context, QueryMapping mapping = null, QueryPolicy policy = null)
            : base(CreateConnection(context.Store.Settings.ConnectionString), HopexLanguage.Default, new HopexMapping(), policy)
        {
            _context = context;
        }

        protected override QueryExecutor CreateExecutor()
        {
            return new HopexExecutor(_context, this);
        }

        public override object Execute(Expression expression)
        {
            return base.Execute(expression);
        }


        class HopexExecutor : SqlQueryProvider.Executor
        {
            private readonly IHopexUnitOfWork _context;

            public HopexExecutor(IHopexUnitOfWork context, SqlQueryProvider provider)
                : base(provider)
            {
                _context = context;
            }

            protected override IEnumerable<T> Project<T>(DbDataReader reader, Func<FieldReader, T> fnProjector, MappingEntity entity, bool closeReader)
            {
                if (typeof(IDomainModelElement).IsAssignableFrom(typeof(T)))
                {
                    try
                    {
                        while (reader.Read())
                        {
                           var row = new DomainModelRow { 
                               Id = reader.GetString(0), 
                               LastModification = reader.IsDBNull(1) ? default(DateTime?) : reader.GetDateTime(1), 
                               Properties = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.GetString(2)) };

                        }
                    }
                    finally
                    {
                        if (closeReader)
                        {
                            ((IDataReader)reader).Close();
                        }
                    }
                }
                else
                {
                    foreach(var row in base.Project(reader, fnProjector, entity, closeReader))
                    {
                        yield return row;
                    }
                }
            }
        }
    }
}
