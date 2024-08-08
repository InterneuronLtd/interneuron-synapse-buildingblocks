 //Interneuron synapse

//Copyright(C) 2024 Interneuron Limited

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

//See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
﻿using System;
using Npgsql;

namespace Interneuron.Infrastructure.CustomExceptions
{
    [Serializable]
    public class InterneuronDBException: Exception
    {
        public string ErrorId { get; private set; }

        public short ErrorCode { get; private set; }

        public string ErrorType { get; private set; }

        public string ErrorMessage { get; private set; }
        public string ErrorResponseMessage { get; private set; }
        public InterneuronDBException(short errorCode = 500, string errorMessage = "", string errorType = "System Error", string errorId = null): base($"Http.{errorCode} {errorType} {errorMessage}")
        {
            this.ErrorId = errorId?? Guid.NewGuid().ToString();
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
            this.ErrorType = errorType;
            this.ErrorResponseMessage = $"Error fetching data from database. Please check the error log with ID: {this.ErrorId} for more details";
        }

        public InterneuronDBException(Exception innerException, short errorCode = 500, string errorMessage = "", string errorType = "System Error",  string errorId = null) : base($"Http.{errorCode} {errorType} {errorMessage}", innerException)
        {
            this.ErrorId = errorId ?? Guid.NewGuid().ToString();
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
            this.ErrorType = errorType;
            this.ErrorResponseMessage = $"Error fetching data from database. Please check the error log with ID: {this.ErrorId} for more details";
        }

        public InterneuronDBException(PostgresException npgEx, short errorCode = 500, string errorMessage = "", string errorType = "System Error",  string errorId = null) : base($"Http.{errorCode} {errorType} {errorMessage}", npgEx)
        {
            this.ErrorId = errorId ?? Guid.NewGuid().ToString();
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
            this.ErrorType = errorType;
            this.ErrorResponseMessage = $"Error fetching data from database. Please check the error log with ID: {this.ErrorId} for more details";
        }
    }
}
