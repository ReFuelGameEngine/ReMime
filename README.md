ReMime - Simple Media Type Resolution
=====================================
ReMime is a very humble library that can identify IANA media types of file
from their file extension and its content. While being fully extensible
with your own resolvers, ReMime will also refer to your operating system's
file type database when resolving files.

Platform Caveats
----------------
* On Windows, the default resolver assumes your application has read access to
  the registry.
* On Linux, not all `/etc/mime.types` syntax is supported.
* None of this was written with MacOS in mind. But maybe it'll work?

Refer to `ReMime.ReFile` as an example of how to use the library. Refer to in line
documentation and the given default resolvers as an example resolver to
implementations.

Contributing
------------
Feel free to contribute your own file type resolvers and bug fixes. The more
file types that can be detected accurately, the better. Currently the
repository is available at https://git.mixedup.dev/ReFuel/ReMime. Accepting [email patches](<mailto:sht7ntgni@mozmail.com>).
