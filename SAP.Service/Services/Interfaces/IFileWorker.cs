using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAP.Service.Services.Interfaces
{
    public interface IFileWorker
    {
        void FileReadWorker();
        void FileWriteWorker();
    }
}
