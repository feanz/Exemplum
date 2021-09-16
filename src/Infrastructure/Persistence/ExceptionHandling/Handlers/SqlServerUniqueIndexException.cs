﻿namespace Exemplum.Infrastructure.Persistence.ExceptionHandling.Handlers
{
    using Application.Common.Exceptions;
    using Microsoft.Data.SqlClient;
    using Sprache;
    using System;
    using static SqlUniqueIndexExceptionGrammar;

    public partial class SqlServerUniqueIndexException : IHandlerSpecificDBException
    {
        private const int ErrorNumber = 2601;

        public bool CanHandle(Exception exception)
        {
            return exception?.InnerException is SqlException { Number: ErrorNumber };
        }

        public void HandleException(Exception exception)
        {
            if (exception?.InnerException is SqlException sqlListException)
            {
                try
                {
                    var (table, field, value) = ErrorMessage.Parse(sqlListException.Message);

                    throw new DatabaseValidationException(field,
                        $"Duplicate entry. An item already exists that has a '{field}' with the value of: '{value}'.", exception);
                }
                catch (ParseException)
                {
                  //its find to just swallow this here as the normal DB exception will bubble up and be handled   
                }
            }
        }
    }
}