# ECR Repository
resource "aws_ecr_repository" "app_repo" {
  name = "espectaculos-web"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "espectaculos-web"
  }
}

# Application Load Balancer
resource "aws_lb" "app_alb" {
  name               = "myapp-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb_sg.id]
  subnets            = [aws_subnet.public_a.id, aws_subnet.public_b.id]

  tags = {
    Name = "myapp-alb"
  }
}

# EKS
resource "aws_eks_cluster" "main" {
  name     = "eks-lab-cluster"
  role_arn = "arn:aws:iam::466060356317:role/c186660a4830571l12339800t1w466060-LabEksClusterRole-UAoaMXxwnHCl"

  vpc_config {
    subnet_ids = [aws_subnet.public_a.id, aws_subnet.public_b.id]
  }

  version = "1.29"
}

# 🔹 Crear node group (opcional)
resource "aws_eks_node_group" "default" {
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = "eks-lab-nodes"
  node_role_arn   = "arn:aws:iam::466060356317:role/c186660a4830571l12339800t1w466060356-LabEksNodeRole-jHAS2pPr2WQi"

  subnet_ids = [aws_subnet.public_a.id, aws_subnet.public_b.id]

  scaling_config {
    desired_size = 2
    max_size     = 3
    min_size     = 1
  }

  instance_types = ["t3.medium"]
}