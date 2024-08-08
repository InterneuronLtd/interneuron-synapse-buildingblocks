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
﻿using Interneuron.Infrastructure.CustomExceptions;

namespace Interneuron.Infrastructure.Exceptions.Handlers
{
    public class InterneuronDBExceptionHandler : IExceptionHandler
    {
        private InterneuronDBException dbEx;

        public InterneuronDBExceptionHandler(InterneuronDBException dbEx)
        {
            this.dbEx = dbEx;
        }
        public void HandleExceptionAsync(IntrneuronExceptionHandlerOptions options)
        {
            if (options != null)
                options.OnException?.Invoke(this.dbEx, dbEx.ErrorId, dbEx.ErrorResponseMessage);

            if (options != null)
                options.OnExceptionHandlingComplete?.Invoke(this.dbEx, dbEx.ErrorId);
        }
    }
}
