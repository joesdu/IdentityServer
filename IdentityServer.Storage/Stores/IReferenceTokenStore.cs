﻿using IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer.Storage
{
    public interface IReferenceTokenStore
    {
        Task SaveAsync(IReferenceToken token);
    }
}
