# 测试github actions

## GitHub Actions

### 参考

[GitHub Actions 的工作流语法 - GitHub 文档](https://docs.github.com/zh/actions/using-workflows/workflow-syntax-for-github-actions)

[从 GitLab CI/CD 迁移到 GitHub Actions - GitHub Enterprise Server 3.2 Docs](https://docs.github.com/cn/enterprise-server@3.2/actions/migrating-to-github-actions/migrating-from-gitlab-cicd-to-github-actions#introduction)

### 触发

``` yaml
on:
  # 推送至master分支时触发  
  push:
    branches: [ "master" ]
  # 拉取master分支时触发    
  pull_request:
    branches: [ "master" ]

# 手动运行
  workflow_dispatch
```



### runs-on

| **Runner image**            | **YAML workflow label**            | **Notes**                                                    |
| :-------------------------- | :--------------------------------- | :----------------------------------------------------------- |
| Windows Server 2022         | `windows-latest` or `windows-2022` | The `windows-latest` label currently uses the Windows Server 2022 runner image. |
| Windows Server 2019         | `windows-2019`                     |                                                              |
| Ubuntu 22.04                | `ubuntu-latest` or `ubuntu-22.04`  | The `ubuntu-latest` label currently uses the Ubuntu 22.04 runner image. |
| Ubuntu 20.04                | `ubuntu-20.04`                     |                                                              |
| Ubuntu 18.04 [弃用]         | `ubuntu-18.04`                     | Migrate to `ubuntu-20.04` or `ubuntu-22.04`. For more information, see [this GitHub blog post](https://github.blog/changelog/2022-08-09-github-actions-the-ubuntu-18-04-actions-runner-image-is-being-deprecated-and-will-be-removed-by-12-1-22/). |
| macOS Monterey 12           | `macos-latest` or `macos-12`       | The `macos-latest` label currently uses the macOS 12 runner image. |
| macOS Big Sur 11            | `macos-11`                         |                                                              |
| macOS Catalina 10.15 [弃用] | `macos-10.15`                      | Migrate to `macOS-11` or `macOS-12`. For more information, see [this GitHub blog post](https://github.blog/changelog/2022-07-20-github-actions-the-macos-10-15-actions-runner-image-is-being-deprecated-and-will-be-removed-by-8-30-22/). |