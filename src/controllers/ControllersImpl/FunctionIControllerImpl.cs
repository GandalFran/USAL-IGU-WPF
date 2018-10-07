﻿using IGUWPF.src.IO;
using IGUWPF.src.models;
using IGUWPF.src.models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IGUWPF.src.controllers
{
    public class FunctionIControllerImpl : IFunctionController
    {
        private IDAO<Canvas> PlotDAO;
        private IDAO<Function> FunctionDAO;
        private IDataModel<Function> FunctionModel;

        public FunctionIControllerImpl() {
            PlotDAO = new ImageDAOImpl();
            FunctionDAO = new JsonDAOImpl<Function>();
            FunctionModel = new IDataModelImpl<Function>();
        }

        public int AddAndGetID(Function Element)
        {
            return FunctionModel.CreateElement( Element );
        }

        public bool Delete(int ID)
        {
            return FunctionModel.DeleteElement( ID );
        }

        public Function GetById(int ID)
        {
            return FunctionModel.GetElementByID( ID );
        }

        public bool Update(Function Element)
        {
            return FunctionModel.UpdateElement( Element );
        }

        public List<Function> GetAll()
        {
            return FunctionModel.GetAllElements();
        }


        public bool ExportAll(string DataPath)
        {
            return FunctionDAO.ExportMultipleObject( DataPath, FunctionModel.GetAllElements() );
        }

        public bool ImportAll(string DataPath)
        {
            bool result;
            List<Function> toFill = new List<Function>();

            result = FunctionDAO.ImportMultipleObject( DataPath, toFill );

            if (result)
            {
                FunctionModel = new IDataModelImpl<Function>( toFill );
            }
            
            return result;
        }

        public bool ExportPlot(string FilePath,Canvas PlotPanel)
        {
           return  PlotDAO.ExportSingleObject(FilePath,PlotPanel);
        }
    }
}
