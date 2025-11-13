# SG para RDS
resource "aws_security_group" "rds_sg" {
  name        = "rds-sg"
  description = "Allow access to RDS from VPC"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "Postgres access"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/16"] # solo dentro de la VPC
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "rds-sg"
  }
}

# SG para Redis
resource "aws_security_group" "redis_sg" {
  name        = "redis-sg"
  description = "Allow access to Redis from private subnet"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "Redis access"
    from_port   = 6379
    to_port     = 6379
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/16"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "redis-sg"
  }
}

# SG para EKS
resource "aws_security_group" "eks_sg" {
  name   = "ecs-sg"
  vpc_id = aws_vpc.main.id

  ingress {
    description = "Allow traffic from ALB"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    security_groups = [aws_security_group.alb_sg.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "eks-sg"
  }
}


# 🔹 Security Group para el ALB
resource "aws_security_group" "alb_sg" {
  name        = "alb-sg"
  description = "Allow HTTP inbound from internet"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "HTTP from anywhere"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "alb-sg"
  }
}

# 🔹 Security Group para los nodos del EKS
resource "aws_security_group" "nodes_sg" {
  name        = "eks-nodes-sg"
  description = "Allow traffic from ALB to NodePort"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "Allow traffic from ALB to NodePort 30080"
    from_port       = 30080
    to_port         = 30080
    protocol        = "tcp"
    security_groups = [aws_security_group.alb_sg.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "eks-nodes-sg"
  }
}


# 1) leer cada instancia
data "aws_instance" "eks_node_instances" {
  for_each   = toset(data.aws_instances.eks_nodes.ids)
  instance_id = each.value
}

# 2) compilar lista única de SGs de nodos
locals {
  node_sg_ids = distinct(flatten([
    for inst in data.aws_instance.eks_node_instances :
      inst.vpc_security_group_ids
  ]))
}

# 3) crear regla de ingreso por cada SG de nodo permitiendo tráfico desde el ALB SG (puerto nodePort)
resource "aws_security_group_rule" "allow_alb_to_nodes_nodeport" {
  for_each = toset(local.node_sg_ids)

  type                     = "ingress"
  from_port                = 30080
  to_port                  = 30080
  protocol                 = "tcp"
  security_group_id        = each.key                      # target: SG del nodo
  source_security_group_id = aws_security_group.alb_sg.id   # origen: SG del ALB
  description              = "Allow ALB to reach NodePort 30080"
}