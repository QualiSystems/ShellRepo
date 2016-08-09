# ShellRepo

ShellRepo is a cloud-hosted contralized repository of Cloudshell shells. 
It allows shell developers and users to share and consume shells easily. 
ShellRepo supports TOSCA based shells and is powered by [Toscana](http://github.com/QualiSystems/toscana)

## Website

ShellRepo is availbale at [https://shellrepo.apphb.com](https://shellrepo.apphb.com)

## REST API

ShellRepo supports rich RESTfull API, which enables different clients to interact with it. 

* POST api/shells                               - Uploads a new shell or a new version of a shell
* GET api/shells                                - Retrieves shells with paging. This returns the first page: 20 shells.
* GET api/shells/page/{pageNumber}              - Retrieves shells with paging, according to pageNumber (zero based)
* GET api/shells/versions/{shellName}           - Retrieves shell versions
* GET api/shells/search/{text}                  - Retrieves shells that match text in their name and or description
* GET api/shells/download/{shellName}           - Downloads latest version of the shell
* GET api/shells/download/{shellName}/{version} - Downloads specific version of the shell


## Credits

ShellRepo is generously hosted on [AppHarbor](http://appharbor.com) infrastructure 

## License

ShellRepo is released under Apache Version 2.0 License