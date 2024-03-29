--create database Container;
--use Container;

--use master
--drop database Container
--USE master;
--GO
--ALTER DATABASE Container 
--SET SINGLE_USER 
--WITH ROLLBACK IMMEDIATE;
--GO
--DROP DATABASE Container;

create table usuario(
id_usuario int identity,
nombres varchar(50),
apellidos varchar(50),
usuario varchar(50),
correo varchar(50),
nivel varchar(50),
clave varchar(50)
);

alter table usuario add constraint pk_usuario primary key(id_usuario)
create table repositorio(
id_repositorio int identity,
tipo varchar(50),
nombre varchar(50)
);
alter table repositorio add constraint pk_repositorio primary key(id_repositorio)
create table suscripcion(
id_suscripcion int identity,
nivel varchar(50),
id_usuario_creador int,
id_usuario int,
id_repositorio int,
aceptada varchar(2)
);
alter table suscripcion add constraint pk_suscripcion primary key(id_suscripcion)
alter table suscripcion add constraint fk_suscripcion_usuario foreign key(id_usuario) references usuario(id_usuario)
on delete no action on update no action
alter table suscripcion add constraint fk_suscripcion_repositorio foreign key(id_repositorio) references repositorio(id_repositorio)
on delete no action on update no action
alter table suscripcion add constraint fk_suscripcion_usuario_creador foreign key(id_usuario_creador) references usuario(id_usuario)
on delete no action on update no action

create table archivo_s3(
id_archivo int identity,
nombre_archivo_app varchar(50),
nombre_archivo_s3 varchar(50),
nombre_bucket varchar(50),
id_autor int,
clave_archivo varchar(50),
fecha datetime,
peso_bytes int,
tipo varchar(20)
);
alter table archivo_s3 add constraint pk_archivo_s3 primary key(id_archivo)
alter table archivo_s3 add constraint fk_archivo_s3_usuario foreign key(id_autor) references usuario(id_usuario)
on delete no action on update no action

create table referencia(
id_referencia int identity,
id_repositorio int,
vers int,
rama varchar(50),
fecha datetime,
id_usuario_creador int,
id_archivo int,
clave_archivo varchar(50)
);
alter table referencia add constraint pk_referencia primary key(id_referencia)
alter table referencia add constraint fk_referencia_usuario foreign key(id_usuario_creador) references usuario(id_usuario)
on delete no action on update no action
alter table referencia add constraint fk_referencia_repositorio foreign key(id_repositorio) references repositorio(id_repositorio)
on delete no action on update no action
alter table referencia add constraint fk_referencia_archivo foreign key(id_archivo) references archivo_s3(id_archivo)
on delete no action on update no action
create table comentario(
id_comentario int identity,
id_referencia int,
fecha datetime,
contenido text,
id_usuario int
);
alter table comentario add constraint pk_comentario primary key(id_comentario)
alter table comentario add constraint fk_comentario_referencia foreign key(id_referencia) references referencia(id_referencia)
on delete no action on update no action
alter table comentario add constraint fk_comentario_usuario foreign key(id_usuario) references usuario(id_usuario)
on delete no action on update no action

create table link(
id_link int identity,
id_referencia int,
id_usuario_creador int,
nombre varchar(50)
);

alter table link add constraint pk_link primary key(id_link)
alter table link add constraint fk_link_referencia foreign key(id_referencia) references referencia(id_referencia)
on delete no action on update no action

alter table link add constraint fk_link_usuario foreign key(id_usuario_creador) references usuario(id_usuario)
on delete no action on update no action

create table link_app(
id_link_app int identity,
id_referencia int,
id_usuario_creador int,
id_usuario int,
aceptada varchar(2),
nombre_archivo varchar(50)
);

alter table link_app add constraint pk_link_app primary key(id_link_app)
alter table link_app add constraint fk_link_app_usuario foreign key(id_usuario) references usuario(id_usuario)
on delete no action on update no action
alter table link_app add constraint fk_link_app_referencia foreign key(id_referencia) references referencia(id_referencia)
on delete no action on update no action
alter table link_app add constraint fk_link_app_usuario_creador foreign key(id_usuario_creador) references usuario(id_usuario)
on delete no action on update no action

use Container
delete from repositorio
delete from usuario
delete from suscripcion

select * from repositorio
select * from usuario
select * from suscripcion

select * from archivo_s3
select * from comentario