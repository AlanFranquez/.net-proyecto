# SG para RDS
resource "aws_security_group" "rds_sg" {
  name        = "rds-sg"
  description = "Allow access to RDS from VPC"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "Postgres from EKS nodes"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    security_groups = [aws_security_group.nodes_sg.id]
  }

  ingress {
    description = "Postgres from private subnets"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = ["10.0.21.0/24", "10.0.22.0/24"]
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

  ingress {
    description = "Redis from EKS nodes"
    from_port   = 6379
    to_port     = 6379
    protocol    = "tcp"
    security_groups = [aws_security_group.nodes_sg.id]
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

# SG para  ALB
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


# SG para los nodos del EKS
resource "aws_security_group" "nodes_sg" {
  name        = "eks-nodes-sg"
  description = "Allow traffic from ALB to NodePort"
  vpc_id      = aws_vpc.main.id

  # Tráfico desde ALB a NodePort
  ingress {
    description     = "Allow traffic from ALB to NodePort 30080"
    from_port       = 30080
    to_port         = 30080
    protocol        = "tcp"
    security_groups = [aws_security_group.alb_sg.id]
  }

  # Control plane a nodos (kubelet)
  ingress {
    description = "Allow control plane to communicate with nodes (kubelet)"
    from_port   = 10250
    to_port     = 10250
    protocol    = "tcp"
    security_groups = [
      aws_eks_cluster.main.vpc_config[0].cluster_security_group_id
    ]
  }

  # Comunicación entre nodos
  ingress {
    description = "Node-to-node communication"
    from_port   = 0
    to_port     = 65535
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/16"]
  }

  # HTTPS del cluster a nodos
  ingress {
    description = "Cluster SG to nodes on HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    security_groups = [
      aws_eks_cluster.main.vpc_config[0].cluster_security_group_id
    ]
  }

  # CoreDNS necesita esto para funcionar
  ingress {
    description = "Allow nodes to communicate with each other (DNS, etc.)"
    from_port   = 53
    to_port     = 53
    protocol    = "tcp"
    self        = true
  }

  ingress {
    description = "Allow nodes to communicate with each other (DNS UDP)"
    from_port   = 53
    to_port     = 53
    protocol    = "udp"
    self        = true
  }

  egress {
    description = "Allow all outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "eks-nodes-sg"
  }
}

# Reglas adicionales para el Cluster Security Group
resource "aws_security_group_rule" "cluster_to_nodes_https" {
  type                     = "egress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_eks_cluster.main.vpc_config[0].cluster_security_group_id
  source_security_group_id = aws_security_group.nodes_sg.id
  description              = "Allow cluster to communicate with nodes on HTTPS"
}

resource "aws_security_group_rule" "nodes_to_cluster_https" {
  type                     = "ingress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_eks_cluster.main.vpc_config[0].cluster_security_group_id
  source_security_group_id = aws_security_group.nodes_sg.id
  description              = "Allow nodes to communicate with cluster API"
}