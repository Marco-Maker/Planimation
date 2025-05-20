(define (problem problem)
	(:domain )
	(:objects
		floor1 floor2 floor3 - floor
		person1 person2 person3 person4 person5 - person
		elevator1 - elevator
	)
	(:init
		(at-elevator elevator1 floor3)
		(target person1 floor2)
		(at-person person1 floor1)
		(above floor2 floor1)
		(above floor3 floor2)
		(at-person person2 floor1)
		(at-person person4 floor1)
		(at-person person3 floor2)
		(at-person person5 floor3)
		(target person2 floor3)
		(target person3 floor3)
		(target person4 floor2)
		(target person5 floor1)
	)
	(:goal
		(and
			(reached person1)
			(reached person2)
			(reached person3)
			(reached person4)
			(reached person5)
		)
	)
)
