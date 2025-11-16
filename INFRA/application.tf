#  ECR Repository
resource "aws_ecr_repository" "app_repo" {
  name         = "espectaculos-web"
  force_delete = true

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "espectaculos-web"
  }
}

#  Application Load Balancer
resource "aws_lb" "eks_alb" {
  name               = "eks-external-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb_sg.id]
  subnets            = [aws_subnet.public_a.id, aws_subnet.public_b.id]

  tags = {
    Name = "eks-external-alb"
  }
}

#  Target Group (NodePort)
resource "aws_lb_target_group" "eks_tg" {
  name     = "eks-node-tg"
  port     = 30080 # NodePort del servicio de Kubernetes
  protocol = "HTTP"
  vpc_id   = aws_vpc.main.id

  health_check {
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    healthy_threshold   = 2
    unhealthy_threshold = 2
    timeout             = 5
    interval            = 15
    matcher             = "200-399" 
  }
}

#  Listener HTTP (Puerto 80)
resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.eks_alb.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.eks_tg.arn
  }
}

#  EKS Cluster
resource "aws_eks_cluster" "main" {
  name     = "eks-lab-cluster"
  role_arn = "arn:aws:iam::466060356317:role/c186660a4830571l12339800t1w466060-LabEksClusterRole-UAoaMXxwnHCl"

  vpc_config {
    subnet_ids = [aws_subnet.public_a.id, aws_subnet.public_b.id]
  }

  version = "1.29"
}

#  EKS Node Group
resource "aws_eks_node_group" "default" {
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = "eks-lab-nodes"
  node_role_arn   = "arn:aws:iam::466060356317:role/c186660a4830571l12339800t1w466060356-LabEksNodeRole-jHAS2pPr2WQi"

  subnet_ids = [aws_subnet.public_a.id, aws_subnet.public_b.id]

  scaling_config {
    desired_size = 2
    max_size     = 4
    min_size     = 2
  }

  instance_types = ["t3.medium"]
}

# Obtener nombre del AutoScaling Group que crea el NodeGroup
data "aws_autoscaling_groups" "eks_asgs" {
  filter {
    name   = "tag:eks:cluster-name"
    values = [aws_eks_cluster.main.name]
  }
}

data "aws_autoscaling_group" "eks_group" {
  name = element(data.aws_autoscaling_groups.eks_asgs.names, 0)
}

# Obtener instancias (si existen)
data "aws_instances" "eks_nodes" {
  filter {
    name   = "instance-state-name"
    values = ["running"]
  }

  filter {
    name   = "tag:aws:autoscaling:groupName"
    values = [data.aws_autoscaling_group.eks_group.name]
  }
}

# Adjuntar instancias al target group, usando desired_capacity como fallback
resource "aws_lb_target_group_attachment" "nodes" {
  count = (
    length(data.aws_instances.eks_nodes.ids) != 0
    ? length(data.aws_instances.eks_nodes.ids)
    : data.aws_autoscaling_group.eks_group.desired_capacity
  )

  target_group_arn = aws_lb_target_group.eks_tg.arn
  target_id        = element(
    coalesce(data.aws_instances.eks_nodes.ids, ["dummy"]),
    count.index
  )
  port             = 30080

  depends_on = [
    aws_eks_node_group.default
  ]
}
