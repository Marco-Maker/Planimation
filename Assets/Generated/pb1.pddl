(define (problem problem)
	(:domain logistics)
	(:objects
		floor1 floor2 floor3 - floor
		elevator1 elevator2 - elevator
	)
	(:init
		(at-elevator elevator1 floor1)
		(at-elevator elevator2 floor1)
		(above floor2 floor1)
		(above floor3 floor2)
	)
	(:goal
		(and
		)
	)
)
