﻿using System;
using System.Collections.Generic;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using TencentCloud.Common.Profile;
using TencentCloud.Common;
using TencentCloud.Dnspod.V20210323.Models;
using TencentCloud.Dnspod.V20210323;
using Mapster;
using System.Threading.Tasks;
using System.Linq;
using DomainManageTool.Models;
using DomainManageTool.Views;

namespace DomainManageTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private List<RecordDto> _domainRecords;

        public List<RecordDto> DomainRecords
        {
            get => _domainRecords;
            set => SetProperty(ref _domainRecords, value);
        }

        private List<DomainDto> _domainList;

        public List<DomainDto> DomainList
        {
            get => _domainList;
            set => SetProperty(ref _domainList, value);
        }

        private string _domainName;

        public string DomainName
        {
            get => _domainName;
            set => SetProperty(ref _domainName, value);
        }


        public DelegateCommand CreateDomainRecordCommand { get; private set; }

        public DelegateCommand DomainChangedCommand { get; private set; }

        private readonly PlatFormSecret _secret;

        public MainWindowViewModel(PlatFormSecret platFormSecret)
        {
            CreateDomainRecordCommand = new DelegateCommand(ShowCreateDomainRecordWindowDialog);
            DomainChangedCommand = new DelegateCommand(LoadDomainRecordList);
            _secret = platFormSecret;
            DomainList = LoadDomainList();
        }

        private List<DomainDto> LoadDomainList()
        {
            Credential cred = new Credential
            {
                SecretId = _secret.SecretId,
                SecretKey = _secret.SecretKey,
            };
            // 实例化一个client选项，可选的，没有特殊需求可以跳过
            ClientProfile clientProfile = new ClientProfile();
            // 实例化一个http选项，可选的，没有特殊需求可以跳过
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("dnspod.tencentcloudapi.com");
            clientProfile.HttpProfile = httpProfile;

            // 实例化要请求产品的client对象,clientProfile是可选的
            DnspodClient client = new DnspodClient(cred, "", clientProfile);
            // 实例化一个请求对象,每个接口都会对应一个request对象
            DescribeDomainListRequest req = new DescribeDomainListRequest();

            // 返回的resp是一个DescribeDomainListResponse的实例，与请求对象对应
            DescribeDomainListResponse resp = client.DescribeDomainListSync(req);
            TypeAdapterConfig<DomainListItem, DomainDto>.NewConfig().Map(dst => dst.DomainName, src => src.Name);
            var result = resp.DomainList.Adapt<List<DomainDto>>();
            return result;

        }

        private void LoadDomainRecordList()
        {
            Credential cred = new Credential
            {
                SecretId = _secret.SecretId,
                SecretKey = _secret.SecretKey,
            };
            // 实例化一个client选项，可选的，没有特殊需求可以跳过
            ClientProfile clientProfile = new ClientProfile();
            // 实例化一个http选项，可选的，没有特殊需求可以跳过
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("dnspod.tencentcloudapi.com");
            clientProfile.HttpProfile = httpProfile;

            // 实例化要请求产品的client对象,clientProfile是可选的
            DnspodClient client = new DnspodClient(cred, "", clientProfile);
            // 实例化一个请求对象,每个接口都会对应一个request对象
            DescribeRecordListRequest req = new DescribeRecordListRequest();
            req.Domain = DomainName;
            req.SortField = "updated_on";
            req.SortType = "desc";
            // 返回的resp是一个DescribeRecordLineCategoryListResponse的实例，与请求对象对应
            DescribeRecordListResponse resp = client.DescribeRecordListSync(req);

            TypeAdapterConfig<RecordListItem, RecordDto>.NewConfig().Map(dst => dst.Name, src => src.Name);
            DomainRecords = resp.RecordList.Adapt<List<RecordDto>>();
        }

        private void ShowCreateDomainRecordWindowDialog()
        {
            var window = new CreateDomainRecordWindow();
            window.ShowDialog();
        }


    }
}